CREATE INDEX IF NOT EXISTS idx_task_label ON Task(Label);
CREATE INDEX IF NOT EXISTS idx_task_dueat ON Task(DueAt);
CREATE INDEX IF NOT EXISTS idx_task_completed ON Task(IsCompleted);