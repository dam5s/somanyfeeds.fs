drop table if exists articles;
drop table if exists feeds;
drop table if exists users;


create table users
(
  id   bigserial primary key,
  name text
);


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
  feed_url text                  not null,
  content  text                  not null,
  date     timestamp             null,

  unique (url, feed_url)
);

create index articles_urls on articles (url, feed_url);
create index articles_feed_url on articles (feed_url);


insert into users (id, name)
values (1, 'Damo');

select setval('users_id_seq', 1);
