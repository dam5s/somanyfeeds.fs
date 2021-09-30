alter table bookmarks
drop constraint bookmarks_article_id_fkey,
add constraint bookmarks_article_id_fkey
foreign key (article_id) references articles(id) on delete cascade;

alter table read_articles
drop constraint read_articles_article_id_fkey,
add constraint read_articles_article_id_fkey
foreign key (article_id) references articles(id) on delete cascade;
