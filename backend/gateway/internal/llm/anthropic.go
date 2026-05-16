package llm

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"strings"
	"time"
)

const defaultAnthropicMessagesURL = "https://api.anthropic.com/v1/messages"

type anthropicProvider struct {
	apiKey   string
	endpoint string
	client   *http.Client
}

// NewAnthropicProvider returns nil when no key is configured so local
// development can keep using deterministic fallback decisions.
func NewAnthropicProvider(apiKey string) Provider {
	return NewAnthropicProviderWithEndpoint(apiKey, defaultAnthropicMessagesURL)
}

func NewAnthropicProviderWithEndpoint(apiKey string, endpoint string) Provider {
	apiKey = strings.TrimSpace(apiKey)
	if apiKey == "" {
		return nil
	}
	if strings.TrimSpace(endpoint) == "" {
		endpoint = defaultAnthropicMessagesURL
	}

	return &anthropicProvider{
		apiKey:   apiKey,
		endpoint: endpoint,
		client: &http.Client{
			Timeout: 20 * time.Second,
		},
	}
}

func (p *anthropicProvider) Name() string {
	return "anthropic"
}

func (p *anthropicProvider) Chat(ctx context.Context, req ChatRequest) (*ChatResponse, error) {
	maxTokens := req.MaxTokens
	if maxTokens <= 0 {
		maxTokens = 400
	}

	body := anthropicMessageRequest{
		Model:     string(req.Model),
		System:    req.System,
		MaxTokens: maxTokens,
		Messages:  make([]anthropicMessage, 0, len(req.Messages)),
	}
	for _, message := range req.Messages {
		role := strings.TrimSpace(message.Role)
		if role == "" {
			role = "user"
		}
		body.Messages = append(body.Messages, anthropicMessage{
			Role:    role,
			Content: message.Content,
		})
	}

	encoded, err := json.Marshal(body)
	if err != nil {
		return nil, fmt.Errorf("marshal anthropic request: %w", err)
	}

	httpReq, err := http.NewRequestWithContext(ctx, http.MethodPost, p.endpoint, bytes.NewReader(encoded))
	if err != nil {
		return nil, fmt.Errorf("create anthropic request: %w", err)
	}
	httpReq.Header.Set("Content-Type", "application/json")
	httpReq.Header.Set("Accept", "application/json")
	httpReq.Header.Set("x-api-key", p.apiKey)
	httpReq.Header.Set("anthropic-version", "2023-06-01")

	resp, err := p.client.Do(httpReq)
	if err != nil {
		return nil, fmt.Errorf("call anthropic messages api: %w", err)
	}
	defer resp.Body.Close()

	payload, err := io.ReadAll(io.LimitReader(resp.Body, 1<<20))
	if err != nil {
		return nil, fmt.Errorf("read anthropic response: %w", err)
	}
	if resp.StatusCode < http.StatusOK || resp.StatusCode >= http.StatusMultipleChoices {
		return nil, fmt.Errorf("anthropic messages api returned %d: %s", resp.StatusCode, strings.TrimSpace(string(payload)))
	}

	var decoded anthropicMessageResponse
	if err := json.Unmarshal(payload, &decoded); err != nil {
		return nil, fmt.Errorf("decode anthropic response: %w", err)
	}

	var text strings.Builder
	for _, block := range decoded.Content {
		if block.Type == "text" {
			text.WriteString(block.Text)
		}
	}
	if strings.TrimSpace(text.String()) == "" {
		return nil, fmt.Errorf("anthropic response did not include text content")
	}

	return &ChatResponse{
		Provider:     p.Name(),
		Model:        req.Model,
		Content:      text.String(),
		InputTokens:  decoded.Usage.InputTokens,
		OutputTokens: decoded.Usage.OutputTokens,
	}, nil
}

type anthropicMessageRequest struct {
	Model     string             `json:"model"`
	System    string             `json:"system,omitempty"`
	MaxTokens int                `json:"max_tokens"`
	Messages  []anthropicMessage `json:"messages"`
}

type anthropicMessage struct {
	Role    string `json:"role"`
	Content string `json:"content"`
}

type anthropicMessageResponse struct {
	Content []struct {
		Type string `json:"type"`
		Text string `json:"text"`
	} `json:"content"`
	Usage struct {
		InputTokens  int `json:"input_tokens"`
		OutputTokens int `json:"output_tokens"`
	} `json:"usage"`
}
