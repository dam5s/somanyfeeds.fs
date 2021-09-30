insert into users (id, email, name, password_hash)
values (1, 'damo@example.com', 'Damo', '$2a$11$ExRbaoOXuZI61PdZhMauouk/PwZXH84ueRixvKnC0QU8l9QUsexeC'); -- supersecret

select setval('users_id_seq', 1);
