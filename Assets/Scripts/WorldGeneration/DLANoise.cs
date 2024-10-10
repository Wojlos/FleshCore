using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public static class DLANoise {
	public static float maxHeight = 1f;
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int numParticles, int passes) {

		float[,] noiseMap = new float[mapWidth,mapHeight];
		float[,] riverMap = new float[mapWidth,mapHeight];
		float[,] scaledMap = new float[mapWidth,mapHeight];
		Vector2Int[,] movesMap = new Vector2Int[mapWidth,mapHeight];


		int halfWidth = mapWidth / 2;
		int halfHeight = mapHeight / 2;
		Vector2Int seedPosition = new(
			halfWidth,
            halfHeight
		);

		for (int i = 0; i < mapWidth; i++)
		{
			for (int j = 0; j < mapHeight; j++)
			{
				noiseMap[i, j] = 0;
				movesMap[i,j] = new Vector2Int(0,0);
			}
		}
		noiseMap[halfWidth, halfHeight] = passes+1;

		// var scaleDetail = (noiseMap, movesMap);
		
		for(int i = 1; i < passes+1; i ++){
			// noiseMap = AddDetails(noiseMap,((passes+1)-i),i, numParticles*i*i*i);
			noiseMap = AddDetails(noiseMap, ((passes+1)-i), i, numParticles); 

		}
		// for(int i = 1; i < passes+1; i ++){
		// 	noiseMap = BlurScale(noiseMap,2);
		// 	noiseMap = BlurMap(noiseMap);
		// }
			// noiseMap = BlurMap(noiseMap);

		for (int i = 0; i < (noiseMap.GetLength(0)); i++)
		{
			for (int j = 0; j < (noiseMap.GetLength(1)); j++)
			{
				if(noiseMap[i, j] > 0){
					// noiseMap[i,j] = (1-(Mathf.Pow((noiseMap[i,j])/maxHeight,2 )));
					noiseMap[i,j] = (noiseMap[i,j]/(passes));
				}else{
					noiseMap[i,j] = 0;
				}
			}
		}

		return noiseMap;

	}

	public static float[,] GetRivers(float[,] mountainMap, float[,] noiseMap, int value, int level, int numParticles){
		System.Random rng = new();
		
		float maxDistance = 1f;
		int mapWidth = noiseMap.GetLength(0);
		int mapHeight = noiseMap.GetLength(1);

		List<Vector2Int> directions = new List<Vector2Int>(){ Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };
		bool finished = false;
		for (int p = 0; p < numParticles; p ++){
			Vector2Int particlePosition = new(
                rng.Next(mapWidth),
				rng.Next(mapHeight)
            );
			while( !( mountainMap[particlePosition.x, particlePosition.y] > 0)){
				particlePosition = new(
					rng.Next(mapWidth),
					rng.Next(mapHeight)
				);
			}
			float ch = mountainMap[particlePosition.x, particlePosition.y];
			while(true){

				Vector2Int prevPosition = particlePosition;
				
				if(
					prevPosition.x < 2 || prevPosition.y < 2 ||
					prevPosition.x+2 >= mapWidth || prevPosition.y+2 >= mapHeight
				){
					break;
				}
				

				Vector2Int nextMove =  directions[rng.Next(directions.Count)];
				particlePosition += nextMove;

				float nextHeight = mountainMap[(int)particlePosition.x, (int)particlePosition.y];
				if((nextHeight > 0f && noiseMap[(int)particlePosition.x, (int)particlePosition.y] > 0f) || nextHeight == 0f  ){
					
					if(1 >= Vector2Int.Distance(particlePosition, new Vector2Int(mapHeight/2, mapHeight/2))){
						finished = true;
						break;
					}
					noiseMap[(int)prevPosition.x, (int)prevPosition.y] = value;			
					break;

				}
			}
		}
		return noiseMap;
	}

	private static float[,] AddDetails(float[,] noiseMap, int value, int level, int numParticles){
		System.Random rng = new();
		
		int mapWidth = noiseMap.GetLength(0);
		int mapHeight = noiseMap.GetLength(1);
		List<Vector2Int> directions = new List<Vector2Int>(){ Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };

		for (int p = 0; p < numParticles; p ++){
			Vector2Int particlePosition = new(
                rng.Next(mapWidth),
				rng.Next(mapHeight)
            );
			while(true){
				
				Vector2Int nextMove =  directions[rng.Next(4)];
				Vector2Int prevPosition = particlePosition;
				particlePosition += nextMove;
				if(
					prevPosition.x < 2 || prevPosition.y < 2 ||
					prevPosition.x+2 > mapWidth || prevPosition.y+2 > mapHeight
				){
					break;
				}
				float nextHeight = noiseMap[(int)particlePosition.x, (int)particlePosition.y];
				if(nextHeight > 0f && noiseMap[(int)prevPosition.x, (int)prevPosition.y] <= value
				){
					if(
						Mathf.Clamp(noiseMap[(int)particlePosition.x-1, (int)particlePosition.y],0,1) +
						Mathf.Clamp(noiseMap[(int)particlePosition.x+1, (int)particlePosition.y],0,1) +
						Mathf.Clamp(noiseMap[(int)particlePosition.x, (int)particlePosition.y-1],0,1) + 
						Mathf.Clamp(noiseMap[(int)particlePosition.x, (int)particlePosition.y+1],0,1)<=level &&(

						Mathf.Clamp(noiseMap[(int)prevPosition.x-1, (int)prevPosition.y],0,1) +
						Mathf.Clamp(noiseMap[(int)prevPosition.x+1, (int)prevPosition.y],0,1) +
						Mathf.Clamp(noiseMap[(int)prevPosition.x, (int)prevPosition.y-1],0,1) + 
						Mathf.Clamp(noiseMap[(int)prevPosition.x, (int)prevPosition.y+1],0,1)<=level						

						)
							){
							// noiseMap[(int)prevPosition.x, (int)prevPosition.y] = nextHeight + 1f;
							noiseMap[(int)prevPosition.x, (int)prevPosition.y] =  value;

							// movesMap[(int)prevPosition.	x, (int)prevPosition.y] = 1f;
							if(nextHeight > maxHeight){
								maxHeight = nextHeight;
							}			
							break;
							}
					}
				// }
			}
		}
		// for (int i = 1; i < mapWidth-1; i++)
		// {
		// 	for (int j = 1; j < mapHeight-1; j++)
		// 	{
		// 		if(noiseMap[i,j] > 0){
		// 			noiseMap[i, j] = (1-(noiseMap[i, j]/maxHeight));
		// 		}
		// 	}
		// }
		return noiseMap;
	}

	
	private static float[,] GDLADetails(float[,] noiseMap, int numSteps, int numParticles){
		System.Random rng = new();
		
		int mapWidth = noiseMap.GetLength(0);
		int mapHeight = noiseMap.GetLength(1);
		List<Vector2Int> directions = new List<Vector2Int>(){ Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };

		for (int p = 0; p < numParticles; p ++){
			Vector2Int particlePosition = new(
                rng.Next(mapWidth),
				rng.Next(mapHeight)
            );
			// Vector2Int particlePosition = new (mapWidth/2, mapHeight/2);
			// Vector2Int particlePosition = new(
            //     rng.Next(mapWidth),
			// 	rng.Next(mapHeight)
            // );
			// while( !(
			// 	(particlePosition.x < 10f || particlePosition.x > mapWidth -10f) ||
			// 	(particlePosition.y < 10f || particlePosition.y > mapHeight-10f)
			// 	)
			// ){
			// 	particlePosition = new(
            //    		rng.Next(mapWidth),
            //     	rng.Next(mapHeight)
            // 	);
			// }
			bool found = false;
			for(int s = 0; s < numSteps; s ++){
				
				Vector2Int nextMove =  directions[rng.Next(4)];
				Vector2Int prevPosition = particlePosition;
				particlePosition += nextMove;
				if(
					prevPosition.x < 2 || prevPosition.y < 2 ||
					prevPosition.x+2 > mapWidth || prevPosition.y+2 > mapHeight
				){
					found = true;
					break;
				}
				float nextHeight = noiseMap[(int)particlePosition.x, (int)particlePosition.y];
				if(nextHeight > 0f && noiseMap[(int)prevPosition.x, (int)prevPosition.y] == 0f
				){
					noiseMap[(int)prevPosition.	x, (int)prevPosition.y] = 1f;
					found = true;
					break;
				}
				
			}
			if(!found){
				Vector2Int closestPx = new Vector2Int(1, 1); 
				float minDistanceP = (float)mapHeight;
				for (int i = 0; i < (noiseMap.GetLength(0)); i++)
				{
					for (int j = 0; j < (noiseMap.GetLength(1)); j++)
					{
						if(noiseMap[i, j] > 0){
							Vector2Int currentPx = new Vector2Int(i,j);
							float currentDist = Vector2Int.Distance(currentPx, particlePosition);
							if(currentDist < minDistanceP){
								closestPx = currentPx;
								minDistanceP = currentDist;
							}
						}
					}
				}
				List<Vector2Int> neighbors = new List<Vector2Int>();
				if(closestPx.y < mapHeight-1 && noiseMap[closestPx.x,closestPx.y+1] == 0){
					neighbors.Add(new Vector2Int(closestPx.x, closestPx.y + 1));
				}
				if(closestPx.y > 1f && noiseMap[closestPx.x,closestPx.y-1] == 0){
					neighbors.Add(new Vector2Int(closestPx.x, closestPx.y - 1));
				}
				if(closestPx.x < mapWidth-1 && noiseMap[closestPx.x+1,closestPx.y] == 0){
					neighbors.Add(new Vector2Int(closestPx.x+1, closestPx.y));
				}
				if(closestPx.x > 1f && noiseMap[closestPx.x-1,closestPx.y] == 0){
					neighbors.Add(new Vector2Int(closestPx.x-1, closestPx.y));
				}
				
				if(neighbors.Count == 0){
					break;
				}
				Vector2Int newPx = neighbors[0];
				float minDistance = (float)mapHeight;

				foreach (Vector2Int neighbor in neighbors)
				{
					float distance = Vector2.Distance(neighbor, particlePosition);
					if (distance < minDistance)
					{
						minDistance = distance;
						newPx = neighbor;
					}
				}
				noiseMap[newPx.x, newPx.y] = 1f;
			}
		}		
		return noiseMap;
	}

	private static float[,] ScaleUp(float[,] oldMap, float[,] detailMap, int scaleFactor){
		float [,] blurMap = BlurScale(oldMap, scaleFactor);
		// detailMap = BlurScale(detailMap);

		int mapWidth = blurMap.GetLength(0);
		int mapHeight = blurMap.GetLength(1);
		float[,] newMap = new float[mapWidth,mapHeight];

		for (int i = 1; i < mapWidth-1; i++)
		{
			for (int j = 1; j < mapHeight-1; j++)
			{
				newMap[i,j] = (detailMap[i,j] + blurMap[i,j]);
			}
		}
		return detailMap;
	}

	private static (float[,] noiseMap, Vector2Int[,] movesMap ) ScaleDetail(float[,] oldMap, Vector2Int[,] movesMap, int scaleFactor){
		int mapWidth = oldMap.GetLength(0);
		int mapHeight = oldMap.GetLength(1);
		
		float[,] scaledDetailMap = new float[scaleFactor*mapWidth,scaleFactor*mapHeight];
		Vector2Int[,] scaledMovesMap = new Vector2Int[scaleFactor*mapWidth,scaleFactor*mapHeight];

		for (int i = 1; i < mapWidth-1; i++)
		{
			for (int j = 1; j < mapHeight-1; j++)
			{
				int ni = i*scaleFactor;
				int nj = j*scaleFactor;
				
				scaledDetailMap[ni, nj] = 0;
				scaledMovesMap[ni,nj] = new Vector2Int(0,0);

				if(oldMap[i,j] > 0){
					for(int s = -1; s < scaleFactor; s++){
						int sni = ni + ((0-movesMap[i,j].x)*s);
						int snj = nj + ((0-movesMap[i,j].y)*s);
						scaledDetailMap[sni, snj] = oldMap[i,j];
						scaledMovesMap[sni,snj] = movesMap[i,j];
					}
					
				}
			}
		}
		return (scaledDetailMap, scaledMovesMap);

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

	private static float[,] SimpleScale(float[,] oldMap, int scaleFactor){
		
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
				maxHeight =0f;
				// if(oldMap[ni,nj] > 0f){
					float newHeight = 0f;
					if(oldMap[ni-1, nj] > 0){newHeight+=oldMap[ni-1, nj];}else{newHeight+=maxHeight;}
					if(oldMap[ni, nj] > 0){newHeight+=oldMap[ni, nj];}else{newHeight+=maxHeight;}
					if(oldMap[ni-1, nj-1] > 0){newHeight+=oldMap[ni-1, nj-1];}else{newHeight+=maxHeight;}
					if(oldMap[ni, nj-1] > 0){newHeight+=oldMap[ni, nj-1];}else{newHeight+=maxHeight;}
					// scaledNoiseMap[i,j] = ((maxHeight*4)-newHeight)/4;
					scaledNoiseMap[i,j] = (newHeight)/4;

					// scaledNoiseMap[i,j] = (
					// 	(oldMap[ni-1, nj] * (scaleFactor-weightX)) + (oldMap[ni+1, nj]*weightX) +
					// 	(oldMap[ni, nj+1] * (scaleFactor-weightY)) + (oldMap[ni, nj-1]*weightY) 
					// )/4;

				// }
				// if(oldMap[i,j] > 0){
				// 	for(int s = -1; s < scaleFactor; s++){
				// 		int sni = ni + ((0-movesMap[i,j].x)*s);
				// 		int snj = nj + ((0-movesMap[i,j].y)*s);
				// 		scaledDetailMap[sni, snj] = oldMap[i,j];
				// 		scaledMovesMap[sni,snj] = movesMap[i,j];
				// 	}
					
				// }

				
				// if(scaledNoiseMap[i,j] >1 ){
				// 	Debug.Log(scaledNoiseMap[i,j]);
				// }
			}
		}
		return scaledNoiseMap;
	} 

}
