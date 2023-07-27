using UnityEngine;

namespace Code.Data
{
    public class FenString
    {
        public string Fen { get; private set; }
        public string PiecePlacement { get; private set; }
        public bool WhiteToMove { get; private set; }
        public string CastlingRights { get; private set; }
        public int EnPassantIndex { get; private set; }
        public int HalfmoveClock { get; private set; }
        public int FullmoveNumber { get; private set; }

        public FenString() : this("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") { }
        
        public FenString(string fen)
        {
            Fen = fen;
            string[] fenParts = fen.Split(' ');
            PiecePlacement = fenParts[0];
            WhiteToMove = fenParts[1] == "w";
            CastlingRights = fenParts[2];
            EnPassantIndex = GetIndexForSquare(fenParts[3]);
            HalfmoveClock = int.Parse(fenParts[4]);
            FullmoveNumber = int.Parse(fenParts[5]);
        }
        
        public FenString(string piecePlacement, bool whiteToMove, string castlingRights, string enPassantTargetSquare, int halfmoveClock, int fullmoveNumber)
        {
            PiecePlacement = piecePlacement;
            WhiteToMove = whiteToMove;
            CastlingRights = castlingRights;
            EnPassantIndex = GetIndexForSquare(enPassantTargetSquare);;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;
            Fen = $"{piecePlacement} {(whiteToMove ? "w" : "b")} {castlingRights} {enPassantTargetSquare} {halfmoveClock} {fullmoveNumber}";
        }

        public void UpdateFenString()
        {
            Fen = $"{PiecePlacement} {(WhiteToMove ? "w" : "b")} {CastlingRights} {EnPassantIndex} {HalfmoveClock} {FullmoveNumber}";
        }
        
        public int GetIndexForSquare(string square)
        {
            if (square == "-")
                return -1;
            
            int file = square[0] - 'a';
            int rank = square[1] - '1';
            return rank * 8 + file;
        }
        
        public void SetWhiteToMove(bool whiteToMove)
        {
            WhiteToMove = whiteToMove;
            UpdateFenString();
        }
        
        public void SetEnPassantIndex(int index)
        {
            EnPassantIndex = index;
            UpdateFenString();
        }
        
        public bool WhiteCanCastleKingside => CastlingRights.Contains("K");
        
        public bool WhiteCanCastleQueenside => CastlingRights.Contains("Q");
        
        public bool BlackCanCastleKingside => CastlingRights.Contains("k");
        
        public bool BlackCanCastleQueenside => CastlingRights.Contains("q");

        public void SetWhiteCanCastleShort(bool b)
        {
            if (b)
            {
                // TODO: Implement
                throw new System.NotImplementedException();
            }
            else
            {
                CastlingRights = CastlingRights.Replace("K", "");
            }
        }
        
        public void SetWhiteCanCastleLong(bool b)
        {
            if (b)
            {
                // TODO: Implement
                throw new System.NotImplementedException();
            }
            else
            {
                CastlingRights = CastlingRights.Replace("Q", "");
            }
        }
        
        public void SetBlackCanCastleShort(bool b)
        {
            if (b)
            {
                // TODO: Implement
                throw new System.NotImplementedException();
            }
            else
            {
                CastlingRights = CastlingRights.Replace("k", "");
            }
        }
        
        public void SetBlackCanCastleLong(bool b)
        {
            if (b)
            {
                // TODO: Implement
                throw new System.NotImplementedException();
            }
            else
            {
                CastlingRights = CastlingRights.Replace("q", "");
            }
        }

        public void IncrementHalfmoveClock()
        {
            HalfmoveClock++;
            UpdateFenString();
        }
        
        public void ResetHalfmoveClock()
        {
            HalfmoveClock = 0;
            UpdateFenString();
        }
        
        public void IncrementFullmoveNumber()
        {
            FullmoveNumber++;
            UpdateFenString();
        }
    }
}