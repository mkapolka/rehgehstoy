using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Age : MonoBehaviour {
  public static int TEXTURE_COUNT = 4;
  public static int GRASS = 0;
  public static int DIRT = 2;
  public static int ICE = 1;
  public static int STONE = 3;

  public class TerrainPoint {
    public int x;
    public int y;

    public TerrainPoint(int x, int y) {
      this.x = x;
      this.y = y;
    }
  }

  public class TerrainChunk {
    public float[,] data;
    public float[,,] textureData;

    public TerrainChunk(int width, int height) {
      this.data = new float[width, height];
      this.textureData = new float[width, height, TEXTURE_COUNT];
      this.FillTexture(DIRT);
    }

    public TerrainChunk Clone() {
      TerrainChunk output = new TerrainChunk(0, 0);
      output.data = (float[,])this.data.Clone();
      output.textureData = (float[,,])this.textureData.Clone();
      return output;
    }

    public int GetWidth() {
      return this.data.GetLength(0);
    }

    public int GetHeight() {
      return this.data.GetLength(1);
    }

    public TerrainPoint[] RandomlyWithin(int nPoints) {
      TerrainPoint[] points = new TerrainPoint[nPoints];
      for (int i = 0; i < nPoints; i++) {
        int x = (int)(Random.value * this.GetWidth());
        int y = (int)(Random.value * this.GetHeight());
        points[i] = new TerrainPoint(x, y);
      }
      return points;
    }

    public TerrainPoint WalkToMaxima(TerrainPoint start) {
      bool atMaxima = false;
      TerrainPoint last = new TerrainPoint(start.x, start.y);

      while (!atMaxima) {
        TerrainPoint max = new TerrainPoint(last.x, last.y);
        float maxValue = this.data[last.x, last.y];
        for (int x = last.x - 1; x <= last.x + 1; x++) {
          for (int y = last.y - 1; y <= last.y + 1; y++) {
            float v = this.data[x, y];
            if (v > maxValue) {
              maxValue = v;
              max.x = x;
              max.y = y;
            }
          }
        }
        if (max.x == last.x && max.y == last.y) {
          atMaxima = true;
        }
        last.x = max.x;
        last.y = max.y;
      }
      return last;
    }

    public TerrainPoint[] Maxima() {
      List<TerrainPoint> points = new List<TerrainPoint>();
      //int[,] surfaces = new int[this.GetWidth(), this.GetHeight()];
      //List<Vector2D> vectors = new List<Vector2D>();

      for (int x = 1; x < this.GetWidth(); x++) {
        for (int y = 1; y < this.GetHeight(); y++) {
          float v = this.data[x, y];
          bool maxima = true;
          float[] adjacents = this.GetAdjacents(x, y);
          for (int i = 0; i < adjacents.Length; i++) {
            float p = adjacents[i];
            if ((p - v) < 0.001) {
              maxima = false;
            }
          }
          if (maxima) {
            points.Add(new TerrainPoint(x, y));
          }
        }
      }
      return points.ToArray();
    }

    public TerrainPoint GetLinkInPoint() {
      int center_x = this.GetWidth() / 2;
      int center_y = this.GetHeight() / 2;
      int x = center_x;
      int y = center_y;

      int direction = 0;
      int distance = 1;
      int steps = 1;
      bool done = false;
      int n_checks = 0;
      while (!done && n_checks < 2000) {
        if (!(x > 0 && x < this.GetWidth() && y > 0 && y < this.GetHeight())) {
          n_checks += 1;
          continue;
        }

        if (this.data[x, y] > 0.1f && this.GetSlopeAt(x, y) < 0.001f) {
          done = true;
          break;
        } else {
          switch (direction) {
            case 0:
              x += 1;
            break;
            case 1:
              y += 1;
            break;
            case 2:
              x -= 1;
            break;
            case 3:
              y -= 1;
            break;
          }
          n_checks += 1;
          steps -= 1;
          if (steps == 0) {
            if (direction == 1 || direction == 3) {
              distance += 1;
            }
            direction = (direction + 1) % 4;
            steps = distance;
          }
        }
      }
      if (n_checks > this.GetWidth() * this.GetHeight()) {
        Debug.Log("Broke due to too many checks");
      }
      TerrainPoint maxima = this.WalkToMaxima(new TerrainPoint(x, y));
      return maxima;
    }

    public float[] GetAdjacents(int cx, int cy) {
      List<float> points = new List<float>();
      int i = 0;
      for (int x = cx - 1; x <= cx + 1; x++) {
        for (int y = cy - 1; y <= cy + 1; y++) {
          if (!(x == cx && y == cy) && x > 0 && y > 0 && x < this.GetWidth() && y < this.GetHeight()) {
            points.Add(this.data[x, y]);
            i += 1;
          }
        }
      }
      return points.ToArray();
    }

    public static TerrainChunk MakeMountainChunk(int diameter, float height) {
      TerrainChunk output = new TerrainChunk(diameter, diameter);
      int center_x = diameter / 2;
      int center_y = diameter / 2;
      output.Map((x, y) => {
        int dx = (center_x - x);
        int dy = (center_y - y);
        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        distance = 1 - (distance / (diameter / 2));
        return Mathf.Clamp(distance, 0, 1) * height;
      });
      return output;
    }

    public static TerrainChunk MakeShaftChunk(int diameter, float depth) {
      TerrainChunk output = new TerrainChunk(diameter, diameter);
      int center_x = diameter / 2;
      int center_y = diameter / 2;
      output.Map((x, y) => {
        int dx = (center_x - x);
        int dy = (center_y - y);
        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        distance = 1 - (distance / (diameter / 2));
        return 1 - (Mathf.Clamp(distance, 0, 1) * depth);
      });
      return output;
    }

    public void Roughen(float amount, float resolution = 10.0f) {
      float xoffs = Random.value * 20.0f;
      float yoffs = Random.value * 20.0f;
      this.Map((x, y) => {
        float xf = (float)x / this.GetWidth() * resolution;
        float yf = (float)y / this.GetHeight() * resolution;
        return this.data[x, y] + (.5f - Mathf.PerlinNoise(xf + xoffs, yf + yoffs) * 2) * amount * this.data[x, y];
      });
    }

    /*public void Simplify(float amount) {
      int resolution = this.GetWidth() / amount;
      for (int x = 0; x < this.GetWidth(); x += resolution) {
        for (int y = 0; y < this.GetHeight(); y += resolution) {
          float ul = this.data[x, y];
          float lr = this.data[x + resolution, y + resolution];

          for (int x2 = x; x2 < x + resolution; x2++) {
            for (int y2 = y; y2 < y + resolution; y2++) {
              float xv = (x2 - x) / resolution;
              float yv = (y2 - y) / resolution;
              this.data[x2, y2] = this.data
            }
          }
        }
      }
    }*/

    public void HeightBlend(TerrainChunk other) {
      float[] flat = this.data.Cast<float>().ToArray();
      float min = Mathf.Min(flat);
      float max = Mathf.Max(flat);
      float v = Mathf.Clamp(max - min, 0.01f, 1.0f);

      for (int x = 0; x < this.GetWidth(); x++) {
        for (int y = 0; y < this.GetHeight(); y++) {
          float f = (this.data[x, y] - min) / v;
          this.data[x, y] = (this.data[x, y] * (1 - f)) + (other.data[x, y] * f);

          for (int layer = 0; layer < TEXTURE_COUNT; layer++) {
            this.textureData[x, y, layer] = (this.textureData[x, y, layer] * (1 - f)) + (other.textureData[x, y, layer] * f);
          }
        }
      }
    }

    public void Gauntify(float factor) {
      this.Map((x, y) => {
        return this.data[x, y] * this.data[x, y];
      });
    }

    public void Bloat() {
      this.Map((x, y) => {
        return this.data[x, y] + (1 - this.data[x, y]) * .5f;
      });
    }

    public void Map(System.Func<int, int, float> getValue) {
      for (int x = 0; x < this.data.GetLength(0); x++) {
        for (int y = 0; y < this.data.GetLength(1); y++) {
          this.data[x, y] = getValue(x, y);
        }
      }
    }

    public void Max(float max) {
      this.Map((x, y) => Mathf.Max(this.data[x, y], max));
    }

    public void Min(float min) {
      this.Map((x, y) => Mathf.Min(this.data[x, y], min));
    }

    public void Clamp(float min, float max) {
      this.Map((x, y) => Mathf.Clamp(this.data[x, y], min, max));
    }

    public void FillTexture(int whichTexture, float amount = 1.0f) {
      for (int x = 0; x < this.GetWidth(); x++) {
        for (int y = 0; y < this.GetHeight(); y++) {
          float v = amount;
          if (whichTexture == GRASS) {
            v = (1 - this.GetSlopeAt(x, y) * (60 * amount)) * amount;
          }

          for (int layer = 0; layer < TEXTURE_COUNT; layer++) {
            if (layer == whichTexture) {
              this.textureData[x, y, layer] = v;
            } else {
              this.textureData[x, y, layer] = this.textureData[x, y, layer] * (1 - v);
            }
          }
        }
      }
    }

    public void NormalizeTextures() {
      for  (int x = 0; x < this.GetWidth(); x++) {
        for (int y = 0; y < this.GetHeight(); y++) {
          float sum = 0.0f;
          for (int layer = 0; layer < TEXTURE_COUNT; layer++) {
            sum += this.textureData[x, y, layer];
          }
          float q = 1 / sum;
          for (int layer = 0; layer < TEXTURE_COUNT; layer++) {
            this.textureData[x, y, layer] = this.textureData[x, y, layer] * q;
          }
        }
      }
    }

    public float GetSlopeAt(int x, int y) {
      float[] values = this.GetAdjacents(x, y);
      float max = Mathf.Max(values);
      float min = Mathf.Min(values);
      return max - min;
    }

    public enum AddMode {
      Add, Max, Min, RelaMin
    }

    public void AddChunkAtPoints(TerrainPoint[] points, TerrainChunk chunk, AddMode mode = AddMode.Add) {
      for (int i = 0; i < points.Length; i++) {
        int x = points[i].x;
        int y = points[i].y;
        this.AddChunk(chunk, x - chunk.GetWidth() / 2, y - chunk.GetHeight() / 2, mode);
      }
    }

    public void AddChunk(TerrainChunk chunk, int start_x, int start_y, AddMode mode = AddMode.Add) {
      int minWidth = Mathf.Min(chunk.data.GetLength(0), this.data.GetLength(0) - start_x);
      int minHeight = Mathf.Min(chunk.data.GetLength(1), this.data.GetLength(1) - start_y);
      float centerValue = this.data[start_x + chunk.GetWidth() / 2, start_y + chunk.GetHeight() / 2];
      for (int x = 0; x < minWidth; x++) {
        for (int y = 0; y < minHeight; y++) {
          int xc = start_x + x;
          int yc = start_y + y;
          if (xc < 0 || yc < 0) {
            continue;
          }

          switch (mode) {
            case AddMode.Add:
              this.data[xc, yc] = this.data[xc, yc] + chunk.data[x, y];

              if (chunk.data[x, y] != 0) {
                for (int layer = 0; layer < TEXTURE_COUNT; layer++) {
                  this.textureData[xc, yc, layer] = this.textureData[xc, yc, layer] + chunk.textureData[x, y, layer] * 4;
                }
              }
            break;
            case AddMode.Max:
              if (chunk.data[x, y] != 0 && chunk.data[x, y] > this.data[xc, yc]) {
                for (int layer = 0; layer < TEXTURE_COUNT; layer++) {
                  this.textureData[xc, yc, layer] = chunk.textureData[x, y, layer];
                }
              }

              this.data[xc, yc] = Mathf.Max(this.data[xc, yc], chunk.data[x, y]);
            break;
            case AddMode.Min:
              this.data[xc, yc] = Mathf.Min(this.data[xc, yc], chunk.data[x, y]);
            break;
            case AddMode.RelaMin:
              this.data[xc, yc] = Mathf.Min(this.data[xc, yc], centerValue - chunk.data[x, y]);
            break;
          }
        }
      }
    }
  }
}
