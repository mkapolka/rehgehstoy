using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
public class TerrainoTron : MonoBehaviour {

  private TerrainData td;
  private Age.TerrainChunk tc;
  
  public void Start() {
      this.tc = new Age.TerrainChunk(512, 512);
      this.td = this.GetComponent<Terrain>().terrainData;

      this.SetHeights(this.tc);
  }

  public void ApplyChunk(Age.TerrainChunk chunk) {
    this.tc = chunk;
    this.SetHeights(chunk);
    this.SetTextures(chunk);
    this.td.RefreshPrototypes();
    this.GetComponent<Terrain>().Flush();
  }

  public void SetHeights(Age.TerrainChunk chunk) { 
    this.td.SetHeights(0, 0, chunk.data);
  }

  public void SetTextures(Age.TerrainChunk chunk) {
    chunk.NormalizeTextures();
    this.td.SetAlphamaps(0, 0, chunk.textureData);
  }

  public Vector3 TerrainCoordsToPosition(int x, int y) {
    float xs = (float)x / this.tc.GetWidth();
    float ys = (float)y / this.tc.GetHeight();
    float px = this.transform.position.x + (xs * this.td.size.x);
    float pz = this.transform.position.z + (ys * this.td.size.z);
    float py = this.GetComponent<Terrain>().SampleHeight(new Vector3(pz, 0, px));
    return new Vector3(px, this.transform.position.y + py, pz);
  }

  public Vector3 GetLinkInPosition() {
    Age.TerrainPoint tp = this.tc.GetLinkInPoint();
    return this.TerrainCoordsToPosition(tp.x, tp.y);
  }
}
