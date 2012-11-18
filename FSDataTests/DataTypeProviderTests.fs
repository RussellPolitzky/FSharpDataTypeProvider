namespace DataTypeProviderTests

open System
open System.Linq
open System.Data
open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders
open Microsoft.FSharp.Linq
open Microsoft.VisualStudio.TestTools.UnitTesting    
open Assertions

// In these tests, we show off useage of F# 3.0 features including type the provider
// for SQL Server as well as Query Expressions. The type provider instantly provides
// strong typing for the DB schema and allows the user to explore it using intellisense.
// This makes access to data extremely easy and simple for data scripting purposes.

// This beautiful type provider gives us instant, type safe access to database.
type dbSchema = SqlDataConnection<"Data Source=.;Initial Catalog=OrmTest;Integrated Security=SSPI;">

[<TestClass>]
type Test_DataTypeProvider() =

    let db = dbSchema.GetDataContext()
    
    [<TestInitialize>]
    member this.Setup() =
        db.DataContext.Log <- System.Console.Out
        
    //--------------------------------------------------------------------------------------
    [<TestMethod>]        
    member this.``should be able to load data from the db``() =
        query {
            for row in db.Races do
            select row
        }
        |> Seq.map (fun row -> sprintf "%i,%s" row.RaceId row.RaceName)
        |> Seq.toSingleSringWithSep ";"
        |> IsSameStringAs "1,Mutant;2,Human"
        
    //--------------------------------------------------------------------------------------
    [<TestMethod>]        
    member this.``should be able to find the names and locations of all human heroes``() =
        let finalQuery = query {
            for hero in db.Heros do 
            join location in db.Locations on (hero.LocationId ?= location.LocationId)  // Note the nullable comparison operator
            join race     in db.Races     on (hero.RaceId ?= race.RaceId)              // Note the nullable comparison operator
            where (race.RaceName = "Human")
            select (hero.HeroName, location.LocationName)
        }

        printf "%s" (finalQuery.ToString()) // Show the SQL 

        finalQuery
        |> Seq.map (fun result -> sprintf "%s,%s" (fst result) (snd result))
        |> Seq.toSingleSringWithSep ";"
        |> IsSameStringAs @"Hulk,Canadian Rockies;Spider Man,New York, New York;Iron Man,New York, New York"

    //--------------------------------------------------------------------------------------
    [<TestMethod>]        
    member this.``should be able to find the names all heroes having accelerated healing``() =
        let finalQuery = query {
            for hero     in db.Heros do 
            join ha      in db.HeroAbilities on (hero.HeroId = ha.HeroId) 
            join ability in db.Abilities     on (ha.AbilityId = ability.AbilityId)
            where (ability.AbilityName = "Accelerated Healing")
            select (hero.HeroName)
        }

        printf "%s" (finalQuery.ToString()) // Show the SQL 

        finalQuery
        |> Seq.map (fun result -> sprintf "%s" result)
        |> Seq.toSingleSringWithSep ";"
        |> IsSameStringAs @"Wolverine;Spider Man"

    //--------------------------------------------------------------------------------------        
    [<TestMethod>]        
    member this.``should be able to find the names and abilities of all heroes having accelerated healing``() =
        // Note how we defined the sub-query and that the query is lazy evaluated.
        // That is to say, F# will not run the query at this point.
        let heroesWithAccHealing = query {
            for hero     in db.Heros do 
            join ha      in db.HeroAbilities on (hero.HeroId = ha.HeroId) 
            join ability in db.Abilities     on (ha.AbilityId = ability.AbilityId)
            where (ability.AbilityName = "Accelerated Healing")
            select (hero.HeroId)
        }

        let finalQuery = query {
                for hero     in db.Heros do 
                join ha      in db.HeroAbilities on (hero.HeroId = ha.HeroId) 
                join ability in db.Abilities     on (ha.AbilityId = ability.AbilityId)
                where (heroesWithAccHealing.Contains(hero.HeroId))  // Use the sub-query here.
                sortBy hero.HeroName
                thenBy ability.AbilityName
                select (hero.HeroName, ability.AbilityName)
            }

        printf "%s" (finalQuery.ToString()) // Show the SQL 

        finalQuery
        |> Seq.map (fun result -> sprintf "%s, %s" (fst result) (snd result))
        |> Seq.toSingleSringWithSep "\r\n"
        |> IsSameStringAs @"Spider Man, Accelerated Healing
Spider Man, Cling to allmost any surface
Spider Man, Durability
Spider Man, Precognition
Spider Man, Super Agility
Spider Man, Super Speed
Spider Man, Super Stamina
Spider Man, Super Strength
Spider Man, Web Slinging
Wolverine, Accelerated Healing
Wolverine, Retractable Claws
Wolverine, Super-human senses"

    //--------------------------------------------------------------------------------------        
    [<TestMethod>]        
    member this.``should be able to retrieve the names of all villains who've fought the avengers``() =
        // Note how we defined the sub-query and that the query is lazy evaluated.
        // That is to say, F# will not run the query at this point.
        let heroesWithAvengersAsEnemies = query {
            for hero      in db.Heros do 
            join heroTeam in db.HeroTeams on (hero.HeroId = heroTeam.HeroId)
            join team     in db.Teams     on (heroTeam.TeamId = team.TeamId)
            where (team.TeamName = "Avengers")
            select (hero.HeroId)
        }

        let finalQuery = query {
            for hero     in db.Heros do 
            join e       in db.Enemies  on (hero.HeroId = e.HeroId) 
            join villain in db.Villains on (e.VillainId = villain.VillainId)
            where (heroesWithAvengersAsEnemies.Contains(hero.HeroId))
            sortBy hero.HeroName
            select (villain.VillainName)
        }

        printf "%s" (finalQuery.ToString()) // Show the SQL

        finalQuery
        |> Seq.map (fun result -> sprintf "%s" result)
        |> Seq.toSingleSringWithSep "\r\n"
        |> IsSameStringAs @"General Thaddeus ""Thunderbolt"" Ross
Abomination
The Leader
Iron Monger
Spy Master
Green Goblin
Dr Octopus
Venom
Kingpin"

