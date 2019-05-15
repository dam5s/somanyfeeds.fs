delete from articles where date < '2018-12-31';

delete from articles where feed_url not in (select distinct url from feeds);

delete from feed_jobs where feed_url not in (select distinct url from feeds);
