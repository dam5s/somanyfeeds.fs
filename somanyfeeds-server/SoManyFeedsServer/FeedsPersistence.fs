module SoManyFeedsServer.FeedsPersistence

open System.Data.Common
open SoManyFeedsServer.DataSource
open AsyncResult.Operators


type FeedRecord =
    { Id : int64
      UserId : int64
      Name : string
      Url : string
    }


type FeedFields =
    { Name : string
      Url : string
    }


let private mapFeed (record : DbDataRecord) : FeedRecord =
    { Id = record.GetInt64 0
      UserId = record.GetInt64 1
      Name = record.GetString 2
      Url = record.GetString 3
    }


let listUrls (dataSource : DataSource) (_ : unit) : AsyncResult<string list> =
    query dataSource
        """ select distinct url
            from feeds
        """
        []
        (fun record -> record.GetString 0)


let listFeeds (dataSource : DataSource) (userId : int64) : AsyncResult<FeedRecord list> =
    let bindings =
        [ Binding ("@UserId", userId) ]

    query dataSource
        """ select id, user_id, name, url
            from feeds
            where user_id = @UserId
        """
        bindings
        mapFeed


let findFeed (dataSource : DataSource) (userId : int64) (feedId : int64) : Async<FindResult<FeedRecord>> =
    let bindings =
        [
        Binding ("@UserId", userId)
        Binding ("@FeedId", feedId)
        ]

    find dataSource
        """ select id, user_id, name, url
            from feeds
            where id = @FeedId and user_id = @UserId
            limit 1
        """
        bindings
        mapFeed


let createFeed (dataSource : DataSource) (userId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    let bindings =
        [
        Binding ("@UserId", userId)
        Binding ("@Name", fields.Name)
        Binding ("@Url", fields.Url)
        ]

    let mapping =
        fun (record : DbDataRecord) ->
            { Id = record.GetInt64(0)
              UserId = userId
              Name = fields.Name
              Url = fields.Url
            }

    query dataSource
        """ insert into feeds (user_id, name, url)
            values (@UserId, @Name, @Url)
            returning id
        """
        bindings
        mapping
        <!> (List.first >> Option.get)


let updateFeed (dataSource : DataSource) (userId : int64) (feedId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    let bindings =
        [
        Binding ("@FeedId", feedId)
        Binding ("@UserId", userId)
        Binding ("@Name", fields.Name)
        Binding ("@Url", fields.Url)
        ]

    let updatedRecord =
        { Id = feedId
          UserId = userId
          Name = fields.Name
          Url = fields.Url
        }

    update dataSource
        """ update feeds
            set name = @Name, url = @Url
            where user_id = @UserId and id = @FeedId
        """
        bindings
        <!> always updatedRecord


let deleteFeed (dataSource : DataSource) (userId : int64) (feedId : int64) : AsyncResult<unit> =
    let bindings =
        [
        Binding ("@FeedId", feedId)
        Binding ("@UserId", userId)
        ]

    update dataSource
        "delete from feeds where user_id = @UserId and id = @FeedId"
        bindings
        <!> always ()
