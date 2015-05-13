using System.Collections.Generic;
using System.Threading;
using BlockEngine.Objects.Core;
using UnityEngine;

namespace BlockEngine
{
	
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class BlockWorldToMesh : MonoBehaviour
	{
		
		private readonly Dictionary<HasTexture, Rect> uvAtlasMapping = new Dictionary<HasTexture, Rect>();
		private readonly Vector2[] uvOrder = new Vector2[4] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
		private BlockEngine blockEngine;
		private Rect cameraWorld;
		private Thread createMesh = null;
		private List<Vector3> vertices = new List<Vector3>();
		private List<int> indices = new List<int>();
		private List<Vector2> uvs = new List<Vector2>();
		// Use this for initialization
		void Start()
		{
			blockEngine = GetComponentInParent<BlockEngine>();
			
			CreateAtlas();
			Mesh mesh = new Mesh();
			GetComponent<MeshFilter>().mesh = mesh;
			renderer.material.shader = blockEngine.blockShader;
			renderer.material.color = Color.white;
			
		}
		
		private void CreateAtlas()
		{
			List<Texture2D> packedTextures = new List<Texture2D>();
			List<HasTexture> hasTextures = new List<HasTexture>();
			hasTextures.AddRange(blockEngine.blockWorldGenerator.GetAllBlockProperies());
			hasTextures.AddRange(blockEngine.blockWorldGenerator.GetAllBackgroundProperies());
			
			int padding = 2;
            KeyValuePair<Texture2D, Rect[]> result = PackTextures(hasTextures, padding);
			Texture2D atlas = result.Key;
			Rect[] uvRects = result.Value;
			
		
			
			int packedTextureIndex = 0;
			
			foreach (HasTexture hasTexture in hasTextures)
			{
				if (hasTexture.HasTexture())
				{
					uvAtlasMapping.Add(hasTexture, uvRects[packedTextureIndex]);
					packedTextureIndex++;
				}
			}
			if (blockEngine.emptyBackgroundTexture != null)
			{
				uvAtlasMapping.Add(BackgroundProperties.EMPTY, uvRects[packedTextureIndex]);
				packedTextureIndex++;
			}
			
			renderer.material.mainTexture = atlas;
		}
		
		private KeyValuePair<Texture2D, Rect[]> PackTextures(List<HasTexture> packedTextures, int padding)
		{

			int textureCount = packedTextures.Count;
			int atlasWidthCount = Mathf.CeilToInt(Mathf.Sqrt(textureCount));
			int blockWidth = packedTextures[0].GetTexture().width;
            int blockHeight = packedTextures[0].GetTexture().height;
			int atlasWidth = atlasWidthCount * (blockWidth + 2 * padding); ;
			int atlasHeight = ((packedTextures.Count - 1) / atlasWidthCount + 1) * (blockHeight + 2 * padding);
			Texture2D atlas = new Texture2D(atlasWidth, atlasHeight);
			atlas.wrapMode = TextureWrapMode.Clamp;
			
			
			atlas.filterMode = blockEngine.blockTextureFilterMode;
			
			
			List<Rect> uvRects = new List<Rect>();
			
			int widthIndex = 0;
			int heightIndex = 0;
			
			int tileWidth = 2*padding + blockWidth;
			int tileHeight = 2*padding + blockHeight;
			
			foreach (HasTexture hasTexture in packedTextures)
			{
			    Texture2D packedTexture = hasTexture.GetTexture();
			    Texture2D formattedTexture = new Texture2D(packedTexture.width, packedTexture.height);
                formattedTexture.SetPixels(packedTexture.GetPixels());

			    if (hasTexture is BlockProperties)
			    {

                    LerpRow(formattedTexture, new IntVector2(1, 0), new IntVector2(formattedTexture.width - 1, 0), Color.black, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(2, 1), new IntVector2(formattedTexture.width - 1, 1), Color.black, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(3, 2), new IntVector2(formattedTexture.width - 1, 2), Color.black, 0.3f);

                    LerpRow(formattedTexture, new IntVector2(formattedTexture.width - 1, 3), new IntVector2(formattedTexture.width - 1, formattedTexture.height - 2), Color.black, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(formattedTexture.width - 2, 3), new IntVector2(formattedTexture.width - 2, formattedTexture.height - 3), Color.black, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(formattedTexture.width - 3, 3), new IntVector2(formattedTexture.width - 3, formattedTexture.height - 4), Color.black, 0.3f);

                    LerpRow(formattedTexture, new IntVector2(0, 1), new IntVector2(0, formattedTexture.height - 1), Color.white, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(1, 2), new IntVector2(1, formattedTexture.height - 1), Color.white, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(2, 3), new IntVector2(2, formattedTexture.height - 1), Color.white, 0.3f);

                    LerpRow(formattedTexture, new IntVector2(3, formattedTexture.height - 1), new IntVector2(formattedTexture.width - 2, formattedTexture.height - 1), Color.white, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(3, formattedTexture.height - 2), new IntVector2(formattedTexture.width - 3, formattedTexture.height - 2), Color.white, 0.5f);
                    LerpRow(formattedTexture, new IntVector2(3, formattedTexture.height - 3), new IntVector2(formattedTexture.width - 4, formattedTexture.height - 3), Color.white, 0.3f);

			    }



				int x = widthIndex*tileWidth + padding;
				int y = heightIndex*tileHeight + padding;
				atlas.SetPixels(x, y, blockWidth, blockHeight, formattedTexture.GetPixels());


				uvRects.Add(new Rect((float)x / atlasWidth, (float)y / atlasHeight, (float)blockWidth / atlasWidth, (float)blockHeight / atlasHeight));
				
                
                
                widthIndex++;
				if (widthIndex == atlasWidthCount)
				{
					heightIndex++;
					widthIndex = 0;
				}
				for (int i = 0; i < padding; i++)
				{
					atlas.SetPixels(x - padding + i, y, 1, blockHeight, formattedTexture.GetPixels(0, 0, 1, blockHeight));
					atlas.SetPixels(x + blockWidth + i, y, 1, blockHeight, formattedTexture.GetPixels(blockWidth-1, 0, 1, blockHeight));
					atlas.SetPixels(x, y-padding+i, blockWidth, 1, formattedTexture.GetPixels(0, 0, blockWidth, 1));
					atlas.SetPixels(x, y + blockHeight + i, blockWidth, 1, formattedTexture.GetPixels(0, blockHeight-1, blockWidth, 1));
				}
				for (int i = 0; i < padding; i++)
				{
					for (int j = 0; j < padding; j++)
					{
						atlas.SetPixel(x - padding + i, y - padding + j, formattedTexture.GetPixel(0, 0));
						atlas.SetPixel(x + blockWidth + i, y - padding + j, formattedTexture.GetPixel(blockWidth - 1, 0));
						atlas.SetPixel(x - padding + i, y + blockHeight + j, formattedTexture.GetPixel(0, blockHeight - 1));
						atlas.SetPixel(x + blockWidth + i, y + blockHeight + j, formattedTexture.GetPixel(blockWidth - 1, blockHeight - 1));
					}
				}
			}
			atlas.Apply();
			return new KeyValuePair<Texture2D, Rect[]>(atlas, uvRects.ToArray());
			
		}
		
		void Update()
		{
			if (createMesh == null || !createMesh.IsAlive)
			{
				
				Vector2 cameraWorldMin = Camera.main.ViewportToWorldPoint(Camera.main.rect.min);
				Vector2 cameraWorldMax = Camera.main.ViewportToWorldPoint(Camera.main.rect.max);
				Vector2 cameraWorldSize = cameraWorldMax - cameraWorldMin;
				cameraWorld = new Rect(cameraWorldMin.x, cameraWorldMin.y, cameraWorldSize.x, cameraWorldSize.y);

				UpdateTriangles();
				
				
				
				
				Mesh mesh = GetComponent<MeshFilter>().mesh;
				mesh.Clear();
				
				mesh.vertices = vertices.ToArray();
				mesh.triangles = indices.ToArray();
				mesh.uv = uvs.ToArray();
			}
		}

	    private void LerpRow(Texture2D texture, IntVector2 from, IntVector2 to, Color targetColor, float change)
	    {
	        IntVector2 dir = (to - from).Normalized;
	        IntVector2 current = from;
	        if (dir.SqrMagnitude > 1)
	        {
	            return;
	        }
            while (current != to)
	        {
                texture.SetPixel(current.x, current.y, Color.Lerp(texture.GetPixel(current.x, current.y), targetColor, change));
                current += dir;
	        }
            
	    }
		
		private void UpdateTriangles()
		{
			
			vertices.Clear();
			indices.Clear();
			uvs.Clear();
			
			foreach (Corner baseCorner in blockEngine.GetCorners(cameraWorld))
			{
			    Block block = baseCorner.GetBaseBlock();
				HasTexture hasTexture = block.BlockProperties.HasTexture() ? (HasTexture)block.BlockProperties : block.BackgroundProperties;
				if (uvAtlasMapping.ContainsKey(hasTexture))
				{
					Rect rect = uvAtlasMapping[hasTexture];

					indices.Add(vertices.Count);
					indices.Add(vertices.Count + 1);
					indices.Add(vertices.Count + 2);
					
					indices.Add(vertices.Count + 2);
					indices.Add(vertices.Count + 3);
					indices.Add(vertices.Count);
					
					
					for (int i = 0; i < Block.CORNER_OFFSETS.Length; i++)
					{
						Corner corner = block.GetCorner(Block.CORNER_OFFSETS[i]);
						vertices.Add(corner.Position);
						uvs.Add(rect.position + Vector2.Scale(uvOrder[i], rect.size));
					}
				}
				
				//}
				
				
			}
			
			
			
		}
		
		

	}
	
}