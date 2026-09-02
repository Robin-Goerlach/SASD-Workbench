CREATE TABLE IF NOT EXISTS templates (
    id TEXT PRIMARY KEY,
    project_id TEXT NULL,
    profile_key TEXT NOT NULL DEFAULT 'general',
    name TEXT NOT NULL,
    description TEXT NULL,
    entry_type TEXT NOT NULL,
    default_status TEXT NOT NULL DEFAULT 'draft',
    content_markdown TEXT NOT NULL DEFAULT '',
    is_system_template INTEGER NOT NULL DEFAULT 0,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(project_id) REFERENCES projects(id)
);

CREATE TABLE IF NOT EXISTS tags (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    normalized_name TEXT NOT NULL UNIQUE,
    color TEXT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    is_deleted INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS entry_tags (
    entry_id TEXT NOT NULL,
    tag_id TEXT NOT NULL,
    created_at TEXT NOT NULL,
    PRIMARY KEY(entry_id, tag_id),
    FOREIGN KEY(entry_id) REFERENCES entries(id) ON DELETE CASCADE,
    FOREIGN KEY(tag_id) REFERENCES tags(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS attachments (
    id TEXT PRIMARY KEY,
    entry_id TEXT NOT NULL,
    original_file_name TEXT NOT NULL,
    stored_file_name TEXT NOT NULL,
    relative_path TEXT NOT NULL,
    mime_type TEXT NULL,
    file_extension TEXT NULL,
    file_size INTEGER NOT NULL,
    sha256_hash TEXT NOT NULL,
    comment TEXT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    deleted_at TEXT NULL,
    FOREIGN KEY(entry_id) REFERENCES entries(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_templates_project_id ON templates(project_id);
CREATE INDEX IF NOT EXISTS idx_templates_profile_key ON templates(profile_key);
CREATE INDEX IF NOT EXISTS idx_templates_is_deleted ON templates(is_deleted);
CREATE INDEX IF NOT EXISTS idx_tags_is_deleted ON tags(is_deleted);
CREATE INDEX IF NOT EXISTS idx_entry_tags_tag_id ON entry_tags(tag_id);
CREATE INDEX IF NOT EXISTS idx_attachments_entry_id ON attachments(entry_id);
CREATE INDEX IF NOT EXISTS idx_attachments_sha256 ON attachments(sha256_hash);
CREATE INDEX IF NOT EXISTS idx_attachments_is_deleted ON attachments(is_deleted);
