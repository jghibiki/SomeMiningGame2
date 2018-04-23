using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaskOperations {

	public static class DiscreteMaskOps { 

		public static bool[,] GenerateLayerMask(Tile[,] tile_layer){

			bool[,] mask = new bool[tile_layer.GetLength(0), tile_layer.GetLength(1)];	

			for(int i=0; i< tile_layer.GetLength(0); i++){
				for(int j=0; j<tile_layer.GetLength(1); j++){
					mask[i, j] = (tile_layer[i, j] == null);
				}
			}
			return mask;
		}

		public static bool[,] AndLayerMasks(bool[,] mask1, bool[,] mask2){

			bool[,] out_mask = new bool[mask1.GetLength(0), mask1.GetLength(1)];	

			for(int i=0; i< mask1.GetLength(0); i++){
				for(int j=0; j<mask1.GetLength(1); j++){
					out_mask[i, j] = mask1[i, j] && mask2[i, j];
				}
			}
			
			return out_mask;
		}

		public static bool[,] OrLayerMasks(bool[,] mask1, bool[,] mask2){

			bool[,] out_mask = new bool[mask1.GetLength(0), mask1.GetLength(1)];	

			for(int i=0; i< mask1.GetLength(0); i++){
				for(int j=0; j<mask1.GetLength(1); j++){
					out_mask[i, j] = mask1[i, j] || mask2[i, j];
				}
			}

			return out_mask;
		}
	}
}
