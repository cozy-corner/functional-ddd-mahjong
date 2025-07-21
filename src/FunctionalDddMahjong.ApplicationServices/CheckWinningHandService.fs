namespace FunctionalDddMahjong.ApplicationServices

open FunctionalDddMahjong.SharedKernel
open FunctionalDddMahjong.Domain
open FunctionalDddMahjong.DomainServices

module CheckWinningHandService =

    /// Information about a detected yaku (winning pattern)
    type YakuInfo =
        {
            /// Internal name of the yaku (e.g., "Tanyao")
            name: string

            /// Display name in Japanese (e.g., "タンヤオ")
            displayName: string

            /// Number of han (points) for this yaku
            han: int

            /// Description of the yaku
            description: string
        }

    /// Response from the winning hand check
    type CheckWinningHandResponse =
        {
            /// Whether the hand is a winning hand
            isWinningHand: bool

            /// List of detected yaku (empty if not winning)
            detectedYaku: YakuInfo list

            /// Total han count (0 if not winning)
            totalHan: int
        }

    /// Converts domain Yaku to Application Service YakuInfo
    let private toYakuInfo (yaku: Yaku.Yaku) : YakuInfo =
        let displayName, description =
            match yaku with
            | Yaku.Tanyao -> "タンヤオ", "中張牌のみで構成された手"
            | Yaku.Pinfu -> "ピンフ", "順子4つと雀頭で構成された手"
            | Yaku.Toitoi -> "トイトイ", "刻子4つで構成された手"
            | Yaku.Honitsu -> "ホンイツ", "一色と字牌で構成された手"
            | Yaku.Chinitsu -> "チンイツ", "一色のみで構成された手"
            | Yaku.Iipeikou -> "一盃口", "同じ順子が2組ある手"

        { name = yaku.ToString()
          displayName = displayName
          han = Yaku.getHan yaku
          description = description }

    /// Analyzes a winning hand and returns the response
    let checkWinningHand (hand: Hand.Hand) : CheckWinningHandResponse =
        // Check if hand is winning
        let isWinning = Hand.isWinningHand hand

        if isWinning then
            // Analyze yaku if winning
            match YakuAnalyzer.analyzeYaku hand with
            | Ok yakuResult ->
                let yakuInfos =
                    yakuResult.Yaku |> List.map toYakuInfo

                { isWinningHand = true
                  detectedYaku = yakuInfos
                  totalHan = yakuResult.TotalHan }
            | Error _ ->
                // Domain constraint violation (non-winning hand passed to yaku analysis)
                // This case should not occur since we check isWinningHand first
                { isWinningHand = true
                  detectedYaku = []
                  totalHan = 0 }
        else
            // Not a winning hand
            { isWinningHand = false
              detectedYaku = []
              totalHan = 0 }
