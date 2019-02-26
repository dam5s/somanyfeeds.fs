create table feed_jobs
(
  feed_url       text primary key not null,
  started_at     timestamp        null,
  locked_until   timestamp        null,
  completed_at   timestamp        null,
  last_failed_at timestamp        null,
  last_failure   text             not null default ''
);

create index feed_jobs_locked_until on feed_jobs (locked_until);
create index feed_jobs_completed_at on feed_jobs (completed_at);
