using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public static class tDLANoise {

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int numParticles, int passes, List<Vector2Int> seeds = null) {
		if (seeds == null) {
			seeds = new List<Vector2Int>();
		}

		float[,] noiseMap = new float[mapWidth,mapHeight];

		int halfWidth = mapWidth / 2;
		int halfHeight = mapHeight / 2;
		System.Random rng = new();
		Vector2Int seedPosition = new(
			halfWidth,
            halfHeight
		);
		for (int i = 0; i < mapWidth; i++)
		{
			for (int j = 0; j < mapHeight; j++)
			{
				noiseMap[i, j] = 0;
				// if(Vector2Int.Distance(new Vector2Int(i,j), seedPosition) == halfHeight){
				// 	noiseMap[i, j] = 1;	
				// }
				// if(i==0 || i == mapWidth-1 ){
				// }
				// if(j == 0 || j == mapHeight-1){
				// noiseMap[i, j] = 1;
				// }
			}
		}

		if(seeds.Count !=0){
			foreach(var seed in seeds){
				noiseMap[seed.x, seed.y] = 1f;
			} 
		}else{
			noiseMap[seedPosition.x, seedPosition.y] = 1;
		}
		
		// noiseMap[seedPosition.x-10, seedPosition.y-10] = 1;
		// noiseMap[seedPosition.x+10, seedPosition.y-10] = 1;


		List<Vector2Int> directions = new List<Vector2Int>(){ Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };
		

		float maxDistance = 0f;
		float maxHeight = 1f;

		for (int p = 0; p < numParticles; p ++){
			Vector2Int particlePosition = new(
                rng.Next(mapWidth),
				rng.Next(mapHeight)
            );
			bool exit = false;
			while( !(
				Vector2Int.Distance(particlePosition, seedPosition) >= (maxDistance) &&
				Vector2Int.Distance(particlePosition, seedPosition) <= maxDistance+10 )){
				particlePosition = new(
               		rng.Next(mapWidth),
                	rng.Next(mapHeight)
            	);
			}
			while(true){
				
				List<Vector2Int> newDirections = new List<Vector2Int>();
				newDirections = directions;
				Vector2Int nextMove =  newDirections[rng.Next(newDirections.Count)];
				Vector2Int prevPosition = particlePosition;
				particlePosition += nextMove;
				if(
					particlePosition.x < 0 || particlePosition.y < 0 ||
					particlePosition.x >= mapWidth || particlePosition.y >= mapHeight
				){	
					break;
				}
				float nextHeight = noiseMap[(int)particlePosition.x, (int)particlePosition.y];
				if(nextHeight > 0f){
					noiseMap[(int)prevPosition.x, (int)prevPosition.y] = nextHeight + 1f;
					if(nextHeight > maxHeight){
						maxHeight = nextHeight;
					}
					float currentDistance = Vector2Int.Distance(seedPosition, prevPosition); 
					if(maxDistance < currentDistance){
						maxDistance = currentDistance;
						if(maxDistance >= (halfWidth)){
							exit = true;
							break;
						}
					}					
					break;
				}
			}
			if(exit){
				break;
			}
		}
		for (int i = 0; i < mapWidth; i++)
		{
			for (int j = 0; j < mapHeight; j++)
			{
				if(noiseMap[i,j] > 0){
					noiseMap[i, j] = (1-(noiseMap[i, j]/maxHeight));
				}
			}
		}

		for(int i = 1; i < passes; i ++){
			noiseMap=BlurScale(noiseMap,2);
			noiseMap=BlurMap(noiseMap);
		}
		noiseMap=BlurMap(noiseMap);

		return noiseMap;
	}

	private static float[,] BlurMap(float[,] oldMap){
		int mapWidth = oldMap.GetLength(0);
		int mapHeight = oldMap.GetLength(1);
		
		float[,] bluredNoiseMap =  new float[mapWidth, mapHeight];
		for (int i = 1; i < mapWidth-1; i++)
		{
			for (int j = 1; j < mapHeight-1; j++)
			{
				bluredNoiseMap[i,j] = ( oldMap[i, j] + oldMap[i, j-1] + oldMap[i, j+1] + oldMap[i-1, j] + oldMap[i+1, j])/5;
			}
		}
		return bluredNoiseMap;
	}

	public static float[,] BlurScale(float[,] oldMap, int scaleFactor){
		
		int mapWidth = oldMap.GetLength(0);
		int mapHeight = oldMap.GetLength(1);
		
		float[,] scaledNoiseMap = new float[scaleFactor*mapWidth,scaleFactor*mapHeight];

		for (int i = scaleFactor; i < (scaleFactor*mapWidth)-(1+scaleFactor); i++)
		{
			for (int j = scaleFactor; j < (scaleFactor*mapHeight)-(1+scaleFactor); j++)
			{
				float weightX = (i%scaleFactor)/scaleFactor;
				float weightY = (j%scaleFactor)/scaleFactor;

				int ni = (int)Mathf.Ceil((i/scaleFactor));
				int nj = (int)Mathf.Ceil((j/scaleFactor));
				if(oldMap[ni,nj] > 0f){
					for(int x = 0; x < scaleFactor; x++){
						int nx = i + x;
						for(int y = 0; y < scaleFactor; y++){
							int ny = j + y;
							scaledNoiseMap[nx,ny] = oldMap[ni,nj];
						}
					}
				}
				// if(oldMap[i,j] > 0){
				// 	for(int s = -1; s < scaleFactor; s++){
				// 		int sni = ni + ((0-movesMap[i,j].x)*s);
				// 		int snj = nj + ((0-movesMap[i,j].y)*s);
				// 		scaledDetailMap[sni, snj] = oldMap[i,j];
				// 		scaledMovesMap[sni,snj] = movesMap[i,j];
				// 	}
					
				// }

				// scaledNoiseMap[i,j] = ( 
				// 	(oldMap[ni-1, nj] * (scaleFactor-weightX)) + (oldMap[ni+1, nj]*weightX) +
				//  	(oldMap[ni, nj+1] * (scaleFactor-weightY)) + (oldMap[ni, nj-1]*weightY) 
				// 	)/4;
				// if(scaledNoiseMap[i,j] >1 ){
				// 	Debug.Log(scaledNoiseMap[i,j]);
				// }
			}
		}
			return scaledNoiseMap;

	} 

	private static float[,] ScaleUp(float[,] oldMap, int scaleFactor){
		
		int mapWidth = oldMap.GetLength(0);
		int mapHeight = oldMap.GetLength(1);
		
		float[,] bluredNoiseMap =  new float[scaleFactor*mapWidth,scaleFactor*mapHeight];

		float[,] scaledNoiseMap = new float[scaleFactor*mapWidth,scaleFactor*mapHeight];

		for (int i = scaleFactor; i < (scaleFactor*mapWidth)-(1+scaleFactor); i++)
		{
			for (int j = scaleFactor; j < (scaleFactor*mapHeight)-(1+scaleFactor); j++)
			{
				int ni = (int)Mathf.Ceil((i/scaleFactor));
				int nj = (int)Mathf.Ceil((j/scaleFactor));
				
				scaledNoiseMap[i,j] = ( oldMap[ni-1, nj-1] + oldMap[ni+1, nj+1] + oldMap[ni-1, nj+1] + oldMap[ni+1, nj-1])/4;
				// if(bluredNoiseMap[i, j] > 0){
					// bluredNoiseMap[i,j] = (1-(bluredNoiseMap[i,j]/maxHeight));
				// }
				
			}
		}

		for (int i = 1; i < (scaleFactor*mapWidth)-1; i++)
		{
			for (int j = 1; j < (scaleFactor*mapHeight)-1; j++)
			{
				bluredNoiseMap[i,j] = ( scaledNoiseMap[i-1, j-1] + scaledNoiseMap[i+1, j+1] + scaledNoiseMap[i-1, j+1] + scaledNoiseMap[i+1, j-1])/5;
				// if(noiseMap[i, j] > 0){
				// 	bluredNoiseMap[i,j] = (1-(bluredNoiseMap[i,j]/maxHeight));
				// }
				
			}
		}
	return bluredNoiseMap;

	} 

}
