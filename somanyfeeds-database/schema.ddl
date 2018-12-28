drop table if exists feeds;
drop table if exists users;

create table users (
  id   bigserial primary key,
  name text
);

create table feeds (
  id        bigserial primary key not null,
  user_id   bigint references users (id) not null,
  name      text not null,
  url       text not null
);

insert into users (id, name) values (1, 'Damo');

select setval('users_id_seq', 1);
