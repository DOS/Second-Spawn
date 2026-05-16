package character

import (
	"context"
	"testing"
	"time"
)

func TestMemoryStoreAddMemoryDeduplicatesSameKindAndSummary(t *testing.T) {
	store := NewMemoryStore()
	now := time.Date(2026, 5, 16, 1, 0, 0, 0, time.UTC)
	store.now = func() time.Time {
		now = now.Add(time.Second)
		return now
	}

	first, err := store.AddMemory(context.Background(), "player-1", MemoryRecord{
		Kind:       MemoryKindPreference,
		Summary:    "Remember this once.",
		Importance: 4,
	})
	if err != nil {
		t.Fatalf("first AddMemory failed: %v", err)
	}

	second, err := store.AddMemory(context.Background(), "player-1", MemoryRecord{
		Kind:       MemoryKindPreference,
		Summary:    " Remember this once. ",
		Importance: 8,
	})
	if err != nil {
		t.Fatalf("second AddMemory failed: %v", err)
	}

	if got := len(second.Body.Memory); got != len(first.Body.Memory) {
		t.Fatalf("expected duplicate memory to update in place, got %d memories before vs %d after", len(first.Body.Memory), got)
	}

	var found MemoryRecord
	for _, memory := range second.Body.Memory {
		if memory.Summary == "Remember this once." {
			found = memory
			break
		}
	}

	if found.ID == "" {
		t.Fatal("expected deduplicated memory to remain present")
	}
	if found.Importance != 8 {
		t.Fatalf("expected higher duplicate importance to win, got %d", found.Importance)
	}
}
