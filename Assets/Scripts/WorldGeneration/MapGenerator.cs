using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class GeneratedTilemap
{
	public bool isResource = false;
	public Tilemap gridTilemap;
}


public class MapGenerator : MonoBehaviour
{
	public int mapWidth;
	public int mapHeight;
	public float noiseScale;
	public int numParticles;
	public int biomeDensity;
	public int passes;
	
	public int surMapWidth;
	public int surMapHeight;
	public float surNoiseScale;
	public int surNumParticles;
	public int surPasses;
	public float surScale;

	[SerializeField]
	private List<GeneratedTilemap> generatedTilemaps = new List<GeneratedTilemap>();
	private List<GeneratedTilemap> resourceTilemaps = new List<GeneratedTilemap>(); 

	public bool autoUpdate;

    [Header("Tilemaps")]
    [SerializeField]
    private Tilemap mainTilemap;
	[SerializeField]
    private Tilemap mainBottom;
	[SerializeField]
    private GameObject paralaxGrid1, paralaxGrid2;
	// [SerializeField]
    private List<Tilemap> surTilemaps = new List<Tilemap>() ;
    [SerializeField]
    private TileBase tile;


	private List<Tilemap> px1SurTilemaps = new List<Tilemap>();
	private Tilemap px1MainTilemap;
	private List<Tilemap> px2SurTilemaps = new List<Tilemap>();
	private Tilemap px2MainTilemap;

    

	public void GenerateMap() {
        float[,] noiseMap = tDLANoise.GenerateNoiseMap (mapWidth, mapHeight, numParticles, passes);
        // float[,] noiseMap = DLANoise.GenerateNoiseMap (mapWidth, mapHeight, numParticles, passes);
		
		// MapDisplay display = FindObjectOfType<MapDisplay> ();
		// display.DrawNoiseMap(noiseMap);
		// Debug.Log(gridTilemapPrefab.GetType());
		surTilemaps.Clear();
		px1SurTilemaps.Clear();
		foreach(GeneratedTilemap gt in generatedTilemaps){
			if(!gt.isResource){
				mainTilemap = gt.gridTilemap;
			}else{
				surTilemaps.Add(gt.gridTilemap);
			}
		}
		if(mainTilemap != null){
			mainTilemap.ClearAllTiles();
			noiseMap = CleanupNoiseMap(noiseMap);
			DrawOnTilemap(noiseMap, mainTilemap, Vector2.zero, true, mainTilemap, surTilemaps);
		}

        Tilemap[] paralaxTms = paralaxGrid1.GetComponentsInChildren<Tilemap>();
		foreach(Tilemap ptm in paralaxTms){
			if(ptm.name == mainTilemap.name){
				px1MainTilemap = ptm;
			}else{
				px1SurTilemaps.Add(ptm);
			}
		}
		px1MainTilemap.ClearAllTiles();
		// px1MainTilemap.GetComponent<Renderer>().material.SetFloat("_Darken", px1Darken);
		DrawParalex(px1MainTilemap, noiseMap,  Vector2.zero,5, true, px1MainTilemap, px1SurTilemaps);
		
		paralaxTms = paralaxGrid2.GetComponentsInChildren<Tilemap>();
		foreach(Tilemap ptm in paralaxTms){
			if(ptm.name == mainTilemap.name){
				px2MainTilemap = ptm;
			}else{
				px2SurTilemaps.Add(ptm);
			}
		}
		px2MainTilemap.ClearAllTiles();
		// px2MainTilemap.GetComponent<Renderer>().material.SetFloat("_Darken", px2Darken);
		DrawParalex(px2MainTilemap, noiseMap, Vector2.zero, 10, true, px2MainTilemap, px2SurTilemaps);

		// paralaxTilemap1
		
		// paralaxTilemap2.ClearAllTiles();
		// paralaxNoise = ScaleDownNoiseMap(paralaxNoise, 5); 
        // DrawOnTilemap(paralaxNoise, paralaxTilemap2, Vector2.zero, true);

		int width = noiseMap.GetLength (0);
		int height = noiseMap.GetLength (1);
		mainBottom.ClearAllTiles();
		FillTilemap(noiseMap,mainBottom );
		int resW = width/biomeDensity;
		int resH = height/biomeDensity;
		
		foreach(var tm in surTilemaps){
			tm.ClearAllTiles();		
			float seed = Random.Range(0,10000);	
		
			float[,] surNoiseMap = new float[width,height];	
			Tilemap px1Tilemap = px1SurTilemaps.FirstOrDefault(tilemap => tilemap.name == tm.name); 
			Tilemap px2Tilemap = px2SurTilemaps.FirstOrDefault(tilemap => tilemap.name == tm.name); 
			
			px1Tilemap.ClearAllTiles();
			px2Tilemap.ClearAllTiles();
			// SetupMainFleshMaterial(px1Tilemap, px1Darken, px1FleshGNoiseScale, px1FleshSpeed);
			// px2Tilemap.GetComponent<Renderer>().material.SetFloat("_Darken", px2Darken);

			
			for(int x = 0; x < resW; x++){
				for(int y = 0; y < resH; y++){
					float sizeFactor = Mathf.PerlinNoise((float)((y + seed + 0.001f)/surScale), (float)((y + seed + 0.001f)/surScale) + seed);
					float[,] surNodeMap = tDLANoise.GenerateNoiseMap ((int)(surMapWidth*sizeFactor), (int)(surMapHeight*sizeFactor), surNumParticles, surPasses);
					Vector2 nodePos =  new Vector2(
						(x * biomeDensity + Random.Range(-biomeDensity/2,biomeDensity/2)),
						(y * biomeDensity + Random.Range(-biomeDensity/2,biomeDensity/2))
					);
					// surNoiseMap = AddToNoiseMap(surNoiseMap, surNodeMap, nodePos);
					// Debug.Log(px1SurTilemaps.FirstOrDefault(tilemap => tilemap.name == tm.name));
					DrawOnTilemap(surNodeMap, tm, nodePos, false, mainTilemap, surTilemaps);
					
					DrawParalex(px1Tilemap, surNodeMap, nodePos, 5, false, px1MainTilemap, px1SurTilemaps);
					DrawParalex(px2Tilemap, surNodeMap, nodePos, 10, false, px2MainTilemap, px2SurTilemaps);
					
					// DrawParalex(px2Tilemap, surNodeMap, nodePos, false, px2MainTilemap, px2SurTilemaps);
					
				}
			}

		}
	}
	void FillTilemap(float[,] noiseMap, Tilemap tilemap){
		int width = noiseMap.GetLength (0);
		int height = noiseMap.GetLength (1);
        for (int x = 1; x < width-1 ; x++) {
			for (int y = 1; y < height-1; y++) {
				Vector3 tilePos = new Vector3(y, x, 0f);
				Vector3Int cellPosition = tilemap.WorldToCell(tilePos);
				tilemap.SetTile(cellPosition, tile);	
			}
		}
	}

	void DrawParalex(Tilemap tm, float[,] nm, Vector2 position, int strength, bool cavern, Tilemap pxMain, List<Tilemap> pxSur){
		float[,] paralaxNoise = nm;
		if(cavern){
			paralaxNoise = ScaleDownNoiseMap(nm, strength, cavern); 
		}
		DrawOnTilemap(paralaxNoise, tm, position, cavern, pxMain, pxSur);
	}

    void DrawOnTilemap(float[,] noiseMap, Tilemap tilemap, Vector2 position, bool cavern, Tilemap pxMain, List<Tilemap> pxSur){
        int width = noiseMap.GetLength (0);
		int height = noiseMap.GetLength (1);
        for (int x = 1; x < width-1 ; x++) {
			for (int y = 1; y < height-1; y++) {
				if(
						((noiseMap[y-1,x] != 0) && (noiseMap[y+1,x] != 0)) ||
						((noiseMap[y,x-1] != 0) && (noiseMap[y,x+1] != 0))
					
					) {
					if((noiseMap[y,x] != 0f) ){
						Vector3 tilePos = new Vector3(y, x, 0f);
						Vector3Int cellPosition = tilemap.WorldToCell(tilePos);
						cellPosition.x += (int)position.x;
						cellPosition.y += (int)position.y;
						if(!cavern){
							if(pxMain.GetTile(cellPosition) == null){
								continue;
							}
							foreach(var tm in pxSur){
									if(tm != tilemap){
										// tmpCP = cellPosition;
										// tmpCP.x -=1;
										// if(tm.GetTile(tmpCP.x))
										tm.SetTile(cellPosition, null);
									}
								}
							// pxMain.SetTile(cellPosition, null);
						}
						tilemap.SetTile(cellPosition, tile);
					}
				}
			}
		}
    }


	float[,] ScaleDownNoiseMap(float[,] oldMap, int thickness, bool cavern){
		int nToLive = 4;
		int width = oldMap.GetLength (0);
		int height = oldMap.GetLength (1);
		float[,] newNoiseMap = new float[width,height];
		List<Vector2Int> directions = new List<Vector2Int>();
		for(int i = -thickness; i <= thickness; i++){
			for(int j = -thickness + Mathf.Abs(i); j <= thickness - Mathf.Abs(i); j++){
				directions.Add(new Vector2Int(i, j ));
			}
		}

		for (int x = 1; x < width-1 ; x++) {
			for (int y = 1; y < height-1; y++) {
				Vector2Int checking = new Vector2Int(x,y);
				int nAlive = 0;
				int val = 0;
				if(cavern){
					val = 1;
				}
				foreach(var direction in directions){
					var checkingDir = checking + direction;
					if((checkingDir.x < 0 || checkingDir.y < 0 ) || (checkingDir.x >= width || checkingDir.y >= height)){
						continue;
					}else{
						if(oldMap[checkingDir.x, checkingDir.y] != 0 ){
							newNoiseMap[x, y] = 1;
							break;
						}
					}
				}
			}
		}
		return newNoiseMap;
	}
	
	float[,] CleanupNoiseMap(float[,] noiseMap){
		int width = noiseMap.GetLength (0);
		int height = noiseMap.GetLength (1);
		float[,] newNoiseMap = new float[width,height];
		List<Vector2Int> toCheck = new List<Vector2Int>();
		List<Vector2Int> directions = new List<Vector2Int>(){ Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };

		for(int y = 0; y <= height; y++){
			if(noiseMap[0, y] == 0){
				toCheck.Add(new Vector2Int(0,y));
				break;
			}		
		}  
		
		newNoiseMap[toCheck[0].x, toCheck[0].y] = 1;
		
		while(toCheck.Count != 0){
			var checking = toCheck[0];
			toCheck.RemoveAt(0);

			foreach(var direction in directions){
				var checkingDir = checking + direction;
				if((checkingDir.x < 0 || checkingDir.y < 0 ) || (checkingDir.x >= width || checkingDir.y >= height)){
					continue;
				}else{
					if(noiseMap[checkingDir.x, checkingDir.y] == 0 && (newNoiseMap[checkingDir.x, checkingDir.y] != 1)){
						newNoiseMap[checkingDir.x, checkingDir.y] = 1;
						toCheck.Add(checkingDir);
					}
				}
			}
		}

		return newNoiseMap;
	}

	void OnValidate() {
		if (mapWidth < 1) {
			mapWidth = 1;
		}
		if (mapHeight < 1) {
			mapHeight = 1;
		}
		// if (lacunarity < 1) {
		// 	lacunarity = 1;
		// }
		// if (octaves < 0) {
		// 	octaves = 0;
		// }
	}
}
