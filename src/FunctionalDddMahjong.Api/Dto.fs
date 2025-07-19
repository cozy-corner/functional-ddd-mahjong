namespace FunctionalDddMahjong.Api

/// DTOs for the winning hand check API
/// Request to check if a hand is winning
type CheckWinningHandRequest =
    {
        /// List of tile strings (e.g., ["1m", "2m", "3m", ...])
        /// Must contain exactly 14 tiles
        tiles: string list
    }

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
