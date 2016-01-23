using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

public class Language : MonoBehaviour {

  public enum WordType {
    Feature, NegativeFeature, Adjective, Adverb, Location
  }

  public const int MAP_SIZE = 512;

	public class Word {
    
  }

  public class Adjective : Word {
    public float quantity;
    public Adverb adverbDelegate = null;

    virtual public void SetQuantity(float v) {
      this.quantity = v;
    }

    virtual public bool CanQualify() {
      return false;
    }

    virtual public void MutateParameters(ChunkParameters parms){}
    virtual public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms){}
  }

  public class FloorWord : Adjective {
    public FloorWord() {
      this.quantity = 0.5f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      if (parms.addMode == Age.TerrainChunk.AddMode.Max) {
        float maxHeight = .12f + (.80f * this.quantity * parms.targetHeight);
        chunk.Clamp(this.quantity * -parms.targetHeight, maxHeight);
      } else {
        chunk.Clamp(this.quantity * -parms.targetHeight, this.quantity * parms.targetHeight);
      }
    }
  }

  public class HeightWord : Adjective {
    public HeightWord(float q = 0.75f) {
      this.quantity = q;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateParameters(ChunkParameters parms) {
      parms.targetHeight = this.quantity;
    }
  }

  public class SizeWord : Adjective {
    public SizeWord() {
      this.quantity = 0.75f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateParameters(ChunkParameters parms) {
      parms.targetRadius = this.quantity;
    }
  }

  public class WarpWord : Adjective {
    public WarpWord() {
      this.quantity = 0.5f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      chunk.Roughen(this.quantity);
    }
  }

  public class ScarredWord : Adjective {
    public ScarredWord() {
      this.quantity = 0.5f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      chunk.Roughen(this.quantity * 0.1f, 50.0f);
    }
  }

  public class IceWord : Adjective {
    public IceWord() {
      this.quantity = 1.0f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      chunk.FillTexture(Age.ICE, this.quantity);
    }
  }

  public class GrassWord : Adjective {
    public GrassWord() {
      this.quantity = 1.0f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      chunk.FillTexture(Age.GRASS, this.quantity);
    }
  }

  public class StoneWord : Adjective {
    public StoneWord() {
      this.quantity = 1.0f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      chunk.FillTexture(Age.STONE, this.quantity);
    }
  }

  public class Adverb : Word {
    virtual public void MutateAdjective(Adjective adjective){}
    virtual public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms){}
  }

  public class NumberAdverb : Adverb {
    public float number = 0.0f;
    public NumberAdverb(float number) {
      this.number = number;
    }

    override public void MutateAdjective(Adjective adjective) {
      if (adjective.CanQualify()) {
        adjective.SetQuantity(this.number);
      }
    }
  }

  public class HeightAdverb : Adverb {
    Adjective adjective = null;
    override public void MutateAdjective(Adjective adjective) {
      adjective.adverbDelegate = this;
      this.adjective = adjective;
    }

    override public void MutateChunk(Age.TerrainChunk chunk, ChunkParameters parms) {
      Age.TerrainChunk dupe = chunk.Clone();
      this.adjective.MutateChunk(dupe, parms);
      chunk.HeightBlend(dupe);
    }
  }

  public class ChunkParameters {
    public float targetRadius;
    public float targetHeight;
    public Age.TerrainChunk.AddMode addMode = Age.TerrainChunk.AddMode.Max;
    public Age.TerrainChunk world = null;
  }

  public class LocationWord : Adjective {
    virtual public Age.TerrainPoint[] GetPoints(ChunkParameters parms) {
      return new Age.TerrainPoint[0];
    }
  }

  public class ScatterWord : LocationWord {
    public ScatterWord() {
      this.quantity = 0.5f;
    }

    override public bool CanQualify() {
      return true;
    }

    override public Age.TerrainPoint[] GetPoints(ChunkParameters parms) {
      int max = (int)(this.quantity * 25);
      Age.TerrainPoint[] output = new Age.TerrainPoint[max];
      for (int n = 0; n < max; n++) {
        float x = (parms.world.GetWidth() / 2) + (Random.value - 0.5f) * (parms.world.GetWidth() * this.quantity / 2);
        float y = (parms.world.GetHeight() / 2) + (Random.value - 0.5f) * (parms.world.GetHeight() * this.quantity / 2);
        output[n] = new Age.TerrainPoint((int)(x), (int)(y));
      }
      return output;
    }
  }

  public class PlaceWord : LocationWord {
    public float x;
    public float y;

    public PlaceWord(float x, float y) {
      this.quantity = 0.25f;
      this.x = x;
      this.y = y;
    }

    override public bool CanQualify() {
      return true;
    }

    override public Age.TerrainPoint[] GetPoints(ChunkParameters parms) {
      int center_x = parms.world.GetWidth() / 2;
      int center_y = parms.world.GetHeight() / 2;
      int x = center_x + (int)(this.x * this.quantity * parms.world.GetWidth() / 2);
      int y = center_y + (int)(this.y * this.quantity * parms.world.GetWidth() / 2);
      return new Age.TerrainPoint[]{new Age.TerrainPoint(x, y)};
    }
  }

  public abstract class FeatureWord : Word {
    public abstract Age.TerrainChunk GetChunk(ChunkParameters parameters);
  }

  public class IslandWord : FeatureWord {
    override public Age.TerrainChunk GetChunk(ChunkParameters parameters) {
      int radius = (int)(MAP_SIZE * parameters.targetRadius);
      Age.TerrainChunk chunk = Age.TerrainChunk.MakeMountainChunk(radius, Mathf.Max(parameters.targetHeight, 7.0f / 25.0f));
      chunk.Roughen(0.5f, 1.5f);
      //chunk.Min(7.0f / 25.0f);
      chunk.Min((6.0f / 25.0f) + ((7.0f / 25.0f) * parameters.targetHeight));
      chunk.Roughen(0.02f, 20.0f);
      return chunk;
    }
  }

  public class MountainWord : FeatureWord {
    override public Age.TerrainChunk GetChunk(ChunkParameters parameters) {
      int radius = (int)(MAP_SIZE * parameters.targetRadius * .5f);
      parameters.addMode = Age.TerrainChunk.AddMode.Add;
      Age.TerrainChunk chunk = Age.TerrainChunk.MakeMountainChunk(radius, parameters.targetHeight * .5f);
      chunk.Roughen(0.2f, 5.0f);
      return chunk;
    }
  }

  public class ShaftWord : FeatureWord {
    override public Age.TerrainChunk GetChunk(ChunkParameters parameters) {
      int radius = (int)(MAP_SIZE * parameters.targetRadius * 0.25);
      parameters.addMode = Age.TerrainChunk.AddMode.Add;
      Age.TerrainChunk output = Age.TerrainChunk.MakeMountainChunk(radius, -parameters.targetHeight);
      return output;
    }
  }

  public static Word ParseWord(string word) {
    switch (word) {
      // Numbers
      case "bfah": // fah (1)
        return new NumberAdverb(1.0f / 25.0f);
      case "bbrE": // bree (2)
        return new NumberAdverb(2.0f / 25.0f);
      case "bsen": // b'sen (3)
        return new NumberAdverb(3.0f / 25.0f);
      case "btor": // tor (4)
        return new NumberAdverb(4.0f / 25.0f);
      case "bvat": // vaht (5)
        return new NumberAdverb(5.0f / 25.0f);
      case "bvagafa": // vagafa (6)
        return new NumberAdverb(6.0f / 25.0f);
      case "bvagabrE": // vagabree (7)
        return new NumberAdverb(7.0f / 25.0f);
      case "bvagasen": // vagasen (8)
        return new NumberAdverb(8.0f / 25.0f);
      case "bvagator": // vagasen (8)
        return new NumberAdverb(9.0f / 25.0f);
      case "bnAvU": // nahvoo (10)
        return new NumberAdverb(10.0f / 25.0f);
      case "bnAgafa":
        return new NumberAdverb(11.0f / 25.0f);
      case "bnAgabrE":
        return new NumberAdverb(12.0f / 25.0f);
      case "bnAgasen":
        return new NumberAdverb(13.0f / 25.0f);
      case "bnAgator":
        return new NumberAdverb(14.0f / 25.0f);
      case "bhEbor":
        return new NumberAdverb(15.0f / 25.0f);
      case "bhEgafa":
        return new NumberAdverb(16.0f / 25.0f);
      case "bhEgabrE":
        return new NumberAdverb(17.0f / 25.0f);
      case "bhEgasen":
        return new NumberAdverb(18.0f / 25.0f);
      case "bhEgator":
        return new NumberAdverb(19.0f / 25.0f);
      case "briS":
        return new NumberAdverb(20.0f / 25.0f);
      case "brigafa":
        return new NumberAdverb(21.0f / 25.0f);
      case "brigabrE":
        return new NumberAdverb(22.0f / 25.0f);
      case "brigasen":
        return new NumberAdverb(23.0f / 25.0f);
      case "brigator":
        return new NumberAdverb(24.0f / 25.0f);
      case "bfasE": // b'(to the) fahsee (25)
        return new NumberAdverb(1.0f);

      case "bantano":
        return new IslandWord();
      case "el":
        return new HeightWord();
      case "elpråD":
        return new MountainWord();
      case "gilot": // gilo + -t -> plant-having???
        return new GrassWord();
      case "minkata":
        return new ScarredWord();
      case "para":
        return new SizeWord();
      case "pråD":
        return new StoneWord();
      case "rema": // Rehmah: north
        return new PlaceWord(0f, 0.9f);
      case "sayra": // Sayrah: west
        return new PlaceWord(-.9f, 0);
      case "tEma": // Teemah: south
        return new PlaceWord(0, -.9f);
      case "tiwa":
        return new ShaftWord();
      case "togot": // togo + -t -> floor-having?
        return new FloorWord();
      case "torinIano": // cold water, ice
        return new IceWord();
      case "trefilad":
        return new HeightAdverb();
      case "tren":
        return new ScatterWord();
      case "vamo": // Vahmo: east
        return new PlaceWord(0.9f, 0);
      case "zEgla": // zeeglah - "warped"
        return new WarpWord();
    }
    return null;
  }

  public static Word[] ParseWords(string text) {
    // Format whitespace
    Regex rgx = new Regex("\\s+");
    text = rgx.Replace(text, " ");
    text = text.Trim();
    if (text == "") {
      return new Word[0];
    }

    List<Word> words = new List<Word>();
    string[] tokens = text.Split(' ');
    foreach (string token in tokens) {
      Word word = ParseWord(token);
      if (word != null) {
        words.Add(word);
      } else {
        throw new System.Exception("Could not parse token \"" + token + "\"");
      }
    }
    return words.ToArray();
  }

  public class Phrase {
    public LocationWord location;
    public Adjective[] adjectives = new Adjective[0];
    public FeatureWord feature;
  }

  private static void ApplyPhrase(Age.TerrainChunk map, Phrase phrase) {
    // Let param changing adjectives have their turn

    ChunkParameters parms = new ChunkParameters();

    parms.targetRadius = 0.5f;
    parms.targetHeight = 0.5f;
    parms.world = map;

    // Default location
    int x = map.GetWidth() / 2;
    int y = map.GetHeight() / 2;

    foreach (Adjective adjective in phrase.adjectives) {
      adjective.MutateParameters(parms);
    }

    Age.TerrainPoint[] points = new Age.TerrainPoint[]{new Age.TerrainPoint(x, y)};
    if (phrase.location != null) {
      points = phrase.location.GetPoints(parms);
    }
    foreach (Age.TerrainPoint point in points) {
      Age.TerrainChunk chunk = phrase.feature.GetChunk(parms);
      foreach (Adjective adjective in phrase.adjectives) {
        if (adjective.adverbDelegate == null) {
          adjective.MutateChunk(chunk, parms);
        } else {
          adjective.adverbDelegate.MutateChunk(chunk, parms);
        }
      }
      int px = point.x - chunk.GetWidth() / 2;
      int py = point.y - chunk.GetHeight() / 2;
      map.AddChunk(chunk, px, py, parms.addMode); 
    }
  }

  public static Age.TerrainChunk GenerateTerrain(string text) {
    Word[] words = ParseWords(text);
    return GenerateTerrain(words);
  }

  public static Phrase[] GetPhrases(Word[] words) {
    List<Phrase> phrases = new List<Phrase>();
    LocationWord currentLocation = null;
    FeatureWord currentFeature = null;
    List<Adjective> currentAdjectives = new List<Adjective>();
    foreach (Word word in words) {
      if (word is FeatureWord && currentFeature != null) {
        Phrase phrase = new Phrase();
        phrase.feature = currentFeature;
        phrase.location = currentLocation;
        phrase.adjectives = currentAdjectives.ToArray();

        phrases.Add(phrase);

        currentLocation = null;
        currentFeature = null;
        currentAdjectives = new List<Adjective>();
      }

      if (word is LocationWord) {
        currentLocation = (LocationWord) word;
        currentAdjectives.Add((Adjective)word);
      } else if (word is FeatureWord) {
        currentFeature = (FeatureWord) word;
      } else if (word is Adjective) {
        currentAdjectives.Add((Adjective)word);
      } else if (word is Adverb) {
        if (currentAdjectives.Count > 0) {
          ((Adverb)word).MutateAdjective(currentAdjectives[currentAdjectives.Count - 1]);
        }
      }
    }
    // Phrasify stragglers
    if (currentFeature != null) {
      Phrase lastPhrase = new Phrase();
      lastPhrase.feature = currentFeature;
      lastPhrase.location = currentLocation;
      lastPhrase.adjectives = currentAdjectives.ToArray();
      phrases.Add(lastPhrase);
    }
    return phrases.ToArray();
  }

  public static Age.TerrainChunk GenerateTerrain(Word[] words) {
    Random.seed = 25;
    Age.TerrainChunk map = new Age.TerrainChunk(512, 512);
    map.FillTexture(Age.DIRT);

    Phrase[] phrases = GetPhrases(words);

    // Lil hack to make bad first noun choices more interesting
    if (phrases.Length == 1 && !(phrases[0].feature is IslandWord)) {
      Phrase islandPhrase = new Phrase();
      islandPhrase.feature = new IslandWord();
      islandPhrase.location = phrases[0].location;
      ApplyPhrase(map, islandPhrase);
    }

    foreach (Phrase phrase in phrases) {
      ApplyPhrase(map, phrase);
    }
    return map;
  }

  public static WordType[] PossibleNextWordTypes(string text) {
    List<WordType> wordTypes = new List<WordType>(new WordType[]{WordType.Feature});

    Word[] words = ParseWords(text);
    if (words.Length == 0) {
      return wordTypes.ToArray();
    } else {
      wordTypes.Add(WordType.NegativeFeature);
      wordTypes.Add(WordType.Adjective);
    }

    Phrase[] phrases = GetPhrases(words);
    if (words[words.Length - 1] is Adjective) {
      Debug.Log(words[words.Length - 1]);
      wordTypes.Add(WordType.Adverb);
    }

    if (phrases[phrases.Length - 1].location == null) {
      wordTypes.Add(WordType.Location);
    }

    return wordTypes.ToArray();
  }
}
