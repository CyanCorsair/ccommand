using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Terrain;

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };
}

public partial class WorldMapGenerator : Node2D
{
    [Export]
    private int MapSizeX = 1024;

    [Export]
    private int MapSizeY = 800;

    private readonly int MaxMapSizeX = 1024;
    private readonly int MaxMapSizeY = 1024;

#nullable enable
    public Tile?[,] tiles;

#nullable disable

    private Random random = new Random();
    private FastNoiseLite biomeNoise = new FastNoiseLite();
    private FastNoiseLite terrainNoise = new FastNoiseLite();
    private BiomeData biomeData = new BiomeData();

    public WorldMapGenerator()
    {
        tiles = new Tile[MaxMapSizeY, MaxMapSizeX];
        biomeNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
        biomeNoise.SetFrequency(0.015f);
        biomeNoise.Seed = 1337;
        biomeNoise.CellularDistanceFunction = FastNoiseLite.CellularDistanceFunctionEnum.Hybrid;
        biomeNoise.CellularReturnType = FastNoiseLite.CellularReturnTypeEnum.CellValue;
        biomeNoise.CellularJitter = 1f;
        biomeNoise.DomainWarpType = FastNoiseLite.DomainWarpTypeEnum.Simplex;
        biomeNoise.DomainWarpAmplitude = 100f;
        biomeNoise.DomainWarpFrequency = 0.01f;
        biomeNoise.DomainWarpFractalType = FastNoiseLite.DomainWarpFractalTypeEnum.Independent;
        biomeNoise.DomainWarpFractalOctaves = 3;
        biomeNoise.DomainWarpFractalLacunarity = 2.0f;
        biomeNoise.DomainWarpFractalGain = 0.5f;

        terrainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        terrainNoise.Seed = 1337;
        terrainNoise.SetFrequency(0.05f);

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        float hexWidth = 24f;
        float hexHeight = 24f;
        int tileX = 0;
        int tileY = 0;

        for (int y = -MapSizeY / (int)hexHeight; y < MapSizeY / (int)hexHeight; y++)
        {
            for (int x = -MapSizeX / (int)hexWidth; x < MapSizeX / (int)hexWidth; x++)
            {
                float offsetX = (y % 2 == 0) ? 0 : hexWidth * 0.5f;
                float startX = x * hexWidth + offsetX;
                float startY = y * (hexHeight * 1f);
                Vector2[] hexagon = GenerateHexagon(startX, startY, hexWidth / 1.75f);
                Tile newTile = GetTileFromHexagon(hexagon, tileX, tileY);
                tiles[tileY, tileX] = newTile;

                if (tileX < MaxMapSizeX - 1)
                {
                    tileX++;
                }
                else
                {
                    tileX = 0;
                    tileY++;
                }
            }
        }
    }

    private Vector2[] GenerateHexagon(float centerX, float centerY, float radius)
    {
        Vector2[] hexagon = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Pi / 3 * i + Mathf.Pi / 6; // Rotate by 30 degrees
            float x = centerX + radius * Mathf.Cos(angle);
            float y = centerY + radius * Mathf.Sin(angle);
            hexagon[i] = new Vector2(x, y);
        }
        return hexagon;
    }

    private Tile GetTileFromHexagon(Vector2[] hexagon, int xIndex, int yIndex)
    {
        TerrainTypes terrain = DetermineTerrainType(xIndex, yIndex);
        return new Tile(xIndex, yIndex, terrain, hexagon);
    }

    private TerrainTypes DetermineTerrainType(int xIndex, int yIndex)
    {
        float biomeNoiseValue = biomeNoise.GetNoise2D(xIndex, yIndex);
        BiomeTypes biome = DetermineBiomeType(biomeNoiseValue);

        float terrainNoiseValue = terrainNoise.GetNoise2D(xIndex, yIndex);
        List<TerrainTypes> possibleTerrains = biomeData.BiomeTerrains[biome];

        return DetermineTerrainWithinBiome(
            possibleTerrains,
            terrainNoiseValue,
            new int[] { xIndex, yIndex }
        );
    }

    private BiomeTypes DetermineBiomeType(float noiseValue)
    {
        float adjustedValue = noiseValue * 5;
        if (adjustedValue < -0.3f)
        {
            return BiomeTypes.Ocean;
        }
        else if (adjustedValue >= 0.3f && adjustedValue <= 0.5f)
        {
            return BiomeTypes.Plains;
        }
        else if (adjustedValue > 0.5f && adjustedValue <= 0.8f)
        {
            return BiomeTypes.Hills;
        }
        else
        {
            return BiomeTypes.Mountains;
        }
    }

    private TerrainTypes DetermineTerrainWithinBiome(
        List<TerrainTypes> possibleTerrains,
        float noiseValue,
        int[] indices
    )
    {
        Tile[] neighbours = GetNeighbours(tiles[indices[1], indices[0]]);
        int mountainsCount = 0;
        int hillsCount = 0;
        int plainsCount = 0;
        int waterCount = 0;

        foreach (Tile neighbour in neighbours)
        {
            if (neighbour == null)
            {
                continue;
            }

            switch (neighbour.GetTerrain())
            {
                case TerrainTypes.Mountains:
                    mountainsCount++;
                    break;
                case TerrainTypes.Hills:
                    hillsCount++;
                    break;
                case TerrainTypes.Plains:
                    plainsCount++;
                    break;
                case TerrainTypes.ShallowWater:
                    waterCount++;
                    break;
            }
        }

        float mountainProbability = mountainsCount * noiseValue;
        float hillsProbability = hillsCount * noiseValue;
        float plainsProbability = plainsCount * noiseValue;
        float waterProbability = waterCount * noiseValue;

        Dictionary<TerrainTypes, float> probabilities = new Dictionary<TerrainTypes, float>
        {
            { TerrainTypes.Mountains, mountainProbability },
            { TerrainTypes.Hills, hillsProbability },
            { TerrainTypes.Plains, plainsProbability },
            { TerrainTypes.ShallowWater, waterProbability }
        };

        probabilities.OrderBy(pair => pair.Value);
        return possibleTerrains.Find((terrain) => probabilities.First().Key == terrain);
    }

    private Tile[] GetNeighbours(Tile tile)
    {
        if (tile == null)
        {
            return new Tile[0];
        }

        int[] tileCoordinates = tile.GetTileXAndY();
        int tileX = tileCoordinates[0];
        int tileY = tileCoordinates[1];

        if (tiles.Length > 1 && tileY % 2 == 0)
        {
            return new Tile[]
            {
                tiles[tileY - 1, tileX - 1],
                tiles[tileY - 1, tileX],
                tiles[tileY, tileX - 1],
                tiles[tileY, tileX + 1],
                tiles[tileY + 1, tileX - 1],
                tiles[tileY + 1, tileX],
            };
        }

        if (tiles.Length > 1 && tileY % 3 == 0)
        {
            return new Tile[]
            {
                tiles[tileY - 1, tileX],
                tiles[tileY - 1, tileX + 1],
                tiles[tileY, tileX - 1],
                tiles[tileY, tileX + 1],
                tiles[tileY + 1, tileX],
                tiles[tileY + 1, tileX + 1],
            };
        }

        return new Tile[0];
    }
}
