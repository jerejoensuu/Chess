using System.Collections.Generic;
using Code.Board;
using Code.Data;
using UnityEngine;

namespace Code.AI
{
    public interface IAI
    {
        public Move GetMove(Square[] squares, FenString fenString, int color);
    }
}