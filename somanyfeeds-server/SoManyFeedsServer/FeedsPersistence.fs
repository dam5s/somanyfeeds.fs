module SoManyFeedsServer.FeedsPersistence

open FeedsProcessing
open System.Data.Common
open SoManyFeedsServer.DataSource


type FeedRecordType =
    | Rss
    | Atom


type FeedRecord =
    { Id : int64
      UserId : int64
      FeedType : FeedRecordType
      Name : string
      Url : string
    }


type FeedFields =
    { UserId : int64
      FeedType : FeedRecordType
      Name : string
      Url : string
    }


let private feedTypeFromString (value : string) : FeedRecordType =
    match value with
    | "Atom" -> Atom
    | _ -> Rss


let private feedTypeToString (value : FeedRecordType) : string =
    match value with
    | Atom -> "Atom"
    | Rss -> "Rss"


let private mapFeed (record : DbDataRecord) : FeedRecord =
    { Id = record.GetInt64 (0)
      UserId = record.GetInt64 (1)
      FeedType = record.GetString (2) |> feedTypeFromString
      Name = record.GetString (3)
      Url = record.GetString (4)
    }


let listFeeds (dataSource: DataSource) : Result<FeedRecord list, string> =
    query dataSource
        """ select id, user_id, feed_type, name, url
            from feeds
        """
        noParams
        mapFeed


let findFeed (dataSource: DataSource) (feedId : int64) : FindResult<FeedRecord> =
    let bindings =
        param "@feedId" feedId

    find dataSource
        """ select id, user_id, feed_type, name, url
            from feeds
            where id = @feedId
            limit 1
        """
        bindings
        mapFeed


let createFeed (dataSource: DataSource) (fields : FeedFields) : Result<FeedRecord, string> =
    let bindings =
        param "@UserId" fields.UserId
        >> param "@FeedType" (feedTypeToString fields.FeedType)
        >> param "@Name" fields.Name
        >> param "@Url" fields.Url

    let mapping =
        fun (record : DbDataRecord) ->
            { Id = record.GetInt64 (0)
              UserId = fields.UserId
              FeedType = fields.FeedType
              Name = fields.Name
              Url = fields.Url
            }

    query dataSource
        """ insert into feeds (user_id, feed_type, name, url)
            values (@UserId, @FeedType, @Name, @Url)
            returning id
        """
        bindings
        mapping
        |> Result.map (List.first >> Option.get)


