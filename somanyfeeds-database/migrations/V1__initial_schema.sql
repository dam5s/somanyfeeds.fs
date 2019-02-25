create table users
(
  id            bigserial primary key,
  email         text unique not null,
  name          text        not null,
  password_hash text        not null
);

create index users_email on users (email);


create table feeds
(
  id      bigserial primary key        not null,
  user_id bigint references users (id) not null,
  name    text                         not null,
  url     text                         not null
);

create index feeds_url on feeds (url);



create table articles
(
  id       bigserial primary key not null,
  url      text                  not null,
  title    text                  not null,
  feed_url text                  not null,
  content  text                  not null,
  date     timestamp             null,

  unique (url, feed_url)
);


create index articles_urls on articles (url, feed_url);
create index articles_feed_url on articles (feed_url);


create table read_articles
(
  user_id bigint references users (id)    not null,
  article_id      bigint references articles (id) not null,

  primary key (user_id, article_id)
);

create index read_articles_user_id on read_articles (user_id);
create index read_articles_article_id on read_articles (article_id);
