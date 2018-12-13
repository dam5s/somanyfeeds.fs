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
    { FeedType : FeedRecordType
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


let listFeeds (dataSource: DataSource) (userId : int64) : Result<FeedRecord list, string> =
    let bindings =
        [ Binding ("@UserId", userId) ]

    query dataSource
        """ select id, user_id, feed_type, name, url
            from feeds
            where user_id = @UserId
        """
        bindings
        mapFeed


let findFeed (dataSource: DataSource) (userId : int64) (feedId : int64) : FindResult<FeedRecord> =
    let bindings =
        [
        Binding ("@UserId" , userId)
        Binding ("@FeedId" , feedId)
        ]

    find dataSource
        """ select id, user_id, feed_type, name, url
            from feeds
            where id = @FeedId and user_id = @UserId
            limit 1
        """
        bindings
        mapFeed


let createFeed (dataSource: DataSource) (userId : int64) (fields : FeedFields) : Result<FeedRecord, string> =
    let bindings =
        [
        Binding ("@UserId", userId)
        Binding ("@FeedType", (feedTypeToString fields.FeedType))
        Binding ("@Name", fields.Name)
        Binding ("@Url", fields.Url)
        ]

    let mapping =
        fun (record : DbDataRecord) ->
            { Id = record.GetInt64(0)
              UserId = userId
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



let updateFeed (dataSource: DataSource) (userId : int64) (feedId : int64) (fields : FeedFields) : Result<FeedRecord, string> =
    let bindings =
        [
        Binding ("@FeedId", feedId)
        Binding ("@UserId", userId)
        Binding ("@FeedType", (feedTypeToString fields.FeedType))
        Binding ("@Name", fields.Name)
        Binding ("@Url", fields.Url)
        ]

    let updatedRecord =
        { Id = feedId
          UserId = userId
          FeedType = fields.FeedType
          Name = fields.Name
          Url = fields.Url
        }

    update dataSource
        """ update feeds
            set feed_type = @FeedType, name = @Name, url = @Url
            where user_id = @UserId and id = @FeedId
        """
        bindings
        |> Result.map (fun _ -> updatedRecord)


let deleteFeed (dataSource: DataSource) (userId : int64) (feedId : int64) : Result<unit, string> =
    let bindings =
        [
        Binding ("@FeedId", feedId)
        Binding ("@UserId", userId)
        ]

    update dataSource
        """ delete from feeds
            where user_id = @UserId and id = @FeedId
        """
        bindings
        |> Result.map (fun _ -> ())
