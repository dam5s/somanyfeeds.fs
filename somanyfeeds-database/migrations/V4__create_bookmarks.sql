create table bookmarks
(
  user_id bigint references users (id)    not null,
  article_id      bigint references articles (id) not null,

  primary key (user_id, article_id)
);

create index bookmarks_user_id on bookmarks (user_id);
create index bookmarks_article_id on bookmarks (article_id);
