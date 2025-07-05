namespace FunctionalDddMahjong.Domain

// 麻雀の基本型定義
module Tile =

    // 数字牌の値（1-9のみ有効）
    type NumberValue =
        | One
        | Two
        | Three
        | Four
        | Five
        | Six
        | Seven
        | Eight
        | Nine

    // 字牌の種類
    type HonorType =
        | East
        | South
        | West
        | North // 風牌
        | White
        | Green
        | Red // 三元牌

    // 牌の種類
    type TileType =
        | Character of NumberValue // 萬子 (Characters/Wan)
        | Circle of NumberValue // 筒子 (Circles/Pin)
        | Bamboo of NumberValue // 索子 (Bamboos/Sou)
        | Honor of HonorType // 字牌 (Honors)

    // 牌を表現する型（Value Object）
    type Tile = private Tile of TileType

    // 牌作成関数（スマートコンストラクタ）
    let create tileType = Tile tileType

    // 牌の内容を取得
    let getValue (Tile tileType) = tileType

    // 比較とソートのためのヘルパー関数
    let getTypeOrder =
        function
        | Character _ -> 1
        | Circle _ -> 2
        | Bamboo _ -> 3
        | Honor _ -> 4

    let getNumberOrder =
        function
        | One -> 1
        | Two -> 2
        | Three -> 3
        | Four -> 4
        | Five -> 5
        | Six -> 6
        | Seven -> 7
        | Eight -> 8
        | Nine -> 9

    let getHonorOrder =
        function
        | East -> 1
        | South -> 2
        | West -> 3
        | North -> 4
        | White -> 5
        | Green -> 6
        | Red -> 7

    // 牌の完全な比較関数
    let compare (Tile t1) (Tile t2) =
        let typeOrder1, typeOrder2 =
            getTypeOrder t1, getTypeOrder t2

        if typeOrder1 <> typeOrder2 then
            compare typeOrder1 typeOrder2
        else
            match t1, t2 with
            | Character n1, Character n2
            | Circle n1, Circle n2
            | Bamboo n1, Bamboo n2 -> compare (getNumberOrder n1) (getNumberOrder n2)
            | Honor h1, Honor h2 -> compare (getHonorOrder h1) (getHonorOrder h2)
            | _ -> 0 // Should never happen due to type safety

    // エラー型定義
    type TileError =
        | InvalidNumberValue of int
        | InvalidHonorName of string
        | InvalidTileString of string

    // Result型を使った安全な牌作成関数
    let tryCreateFromNumber tileType number =
        match number with
        | 1 -> Ok(create (tileType One))
        | 2 -> Ok(create (tileType Two))
        | 3 -> Ok(create (tileType Three))
        | 4 -> Ok(create (tileType Four))
        | 5 -> Ok(create (tileType Five))
        | 6 -> Ok(create (tileType Six))
        | 7 -> Ok(create (tileType Seven))
        | 8 -> Ok(create (tileType Eight))
        | 9 -> Ok(create (tileType Nine))
        | n -> Error(InvalidNumberValue n)

    // 数字牌の文字列解析ヘルパー
    let private tryParseNumberTile (str: string) tileConstructor =
        if str.Length = 2 then
            match System.Int32.TryParse(str.Substring(0, 1)) with
            | (true, n) -> tryCreateFromNumber tileConstructor n
            | _ -> Error(InvalidTileString str)
        else
            Error(InvalidTileString str)

    // 文字列から牌を作成（明確な区別：1文字=風牌・三元牌、2文字=数牌）
    let tryParseFromString (str: string) =
        let upperStr = str.ToUpper()

        match upperStr.Length with
        | 1 ->
            // 風牌・三元牌（1文字）
            match upperStr with
            | "E" -> Ok(create (Honor East))
            | "S" -> Ok(create (Honor South))
            | "W" -> Ok(create (Honor West))
            | "N" -> Ok(create (Honor North))
            | "H" -> Ok(create (Honor White))
            | "G" -> Ok(create (Honor Green))
            | "R" -> Ok(create (Honor Red))
            | _ -> Error(InvalidTileString str)
        | 2 ->
            // 数牌（例：1M, 2P, 3S）
            match upperStr with
            | s when s.EndsWith("M") -> tryParseNumberTile s Character
            | s when s.EndsWith("P") -> tryParseNumberTile s Circle
            | s when s.EndsWith("S") -> tryParseNumberTile s Bamboo
            | _ -> Error(InvalidTileString str)
        | _ -> Error(InvalidTileString str)

    // 牌を文字列で表現（デバッグ用）
    let toString tile =
        match getValue tile with
        | Character n -> sprintf "%d萬" (getNumberOrder n)
        | Circle n -> sprintf "%d筒" (getNumberOrder n)
        | Bamboo n -> sprintf "%d索" (getNumberOrder n)
        | Honor h ->
            match h with
            | East -> "東"
            | South -> "南"
            | West -> "西"
            | North -> "北"
            | White -> "白"
            | Green -> "發"
            | Red -> "中"

    // 牌を英語略記で表現（入力形式と同じ）
    let toShortString tile =
        match getValue tile with
        | Character n -> sprintf "%dm" (getNumberOrder n)
        | Circle n -> sprintf "%dp" (getNumberOrder n)
        | Bamboo n -> sprintf "%ds" (getNumberOrder n)
        | Honor h ->
            match h with
            | East -> "E"
            | South -> "S"
            | West -> "W"
            | North -> "N"
            | White -> "H"
            | Green -> "G"
            | Red -> "R"
