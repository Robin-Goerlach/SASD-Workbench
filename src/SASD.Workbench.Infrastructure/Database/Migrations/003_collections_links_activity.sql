CREATE TABLE IF NOT EXISTS collections (
    id TEXT PRIMARY KEY,
    project_id TEXT NOT NULL,
    parent_collection_id TEXT NULL,
    name TEXT NOT NULL,
    description TEXT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    deleted_at TEXT NULL,
    FOREIGN KEY(project_id) REFERENCES projects(id) ON DELETE CASCADE,
    FOREIGN KEY(parent_collection_id) REFERENCES collections(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS entry_collections (
    entry_id TEXT NOT NULL,
    collection_id TEXT NOT NULL,
    created_at TEXT NOT NULL,
    PRIMARY KEY(entry_id, collection_id),
    FOREIGN KEY(entry_id) REFERENCES entries(id) ON DELETE CASCADE,
    FOREIGN KEY(collection_id) REFERENCES collections(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS entry_links (
    id TEXT PRIMARY KEY,
    source_entry_id TEXT NOT NULL,
    target_entry_id TEXT NOT NULL,
    relation_type TEXT NOT NULL,
    comment TEXT NULL,
    created_at TEXT NOT NULL,
    created_by TEXT NULL,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(source_entry_id) REFERENCES entries(id) ON DELETE CASCADE,
    FOREIGN KEY(target_entry_id) REFERENCES entries(id) ON DELETE CASCADE,
    CHECK(source_entry_id <> target_entry_id)
);

CREATE TABLE IF NOT EXISTS activity_log (
    id TEXT PRIMARY KEY,
    project_id TEXT NULL,
    entry_id TEXT NULL,
    action_type TEXT NOT NULL,
    description TEXT NOT NULL,
    old_value TEXT NULL,
    new_value TEXT NULL,
    created_at TEXT NOT NULL,
    created_by TEXT NULL,
    FOREIGN KEY(project_id) REFERENCES projects(id) ON DELETE SET NULL,
    FOREIGN KEY(entry_id) REFERENCES entries(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_collections_project_id ON collections(project_id);
CREATE INDEX IF NOT EXISTS idx_collections_parent_id ON collections(parent_collection_id);
CREATE INDEX IF NOT EXISTS idx_collections_is_deleted ON collections(is_deleted);
CREATE INDEX IF NOT EXISTS idx_entry_collections_collection_id ON entry_collections(collection_id);
CREATE INDEX IF NOT EXISTS idx_entry_links_source ON entry_links(source_entry_id);
CREATE INDEX IF NOT EXISTS idx_entry_links_target ON entry_links(target_entry_id);
CREATE INDEX IF NOT EXISTS idx_entry_links_is_deleted ON entry_links(is_deleted);
CREATE INDEX IF NOT EXISTS idx_activity_project ON activity_log(project_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_activity_entry ON activity_log(entry_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_activity_created_at ON activity_log(created_at DESC);
