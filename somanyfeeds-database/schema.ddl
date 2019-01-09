drop table if exists articles;
drop table if exists feeds;
drop table if exists users;


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

insert into users (id, email, name, password_hash)
values (1, 'damo@example.com', 'Damo', '$2a$11$ExRbaoOXuZI61PdZhMauouk/PwZXH84ueRixvKnC0QU8l9QUsexeC'); -- supersecret

select setval('users_id_seq', 1);
