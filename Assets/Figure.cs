using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Figure : MonoBehaviour
{
	public GameObject[] prefabArr;
	public float cubeSize = 2f;
	protected float scale = 1f;
	protected float fieldWidth = 0;
//	public Figure() { Clear(); }
	protected int size = 6;
	protected GameObject[,] prevArray;
	protected Vector2 basePoint;
	protected GameObject[,] array;
	protected Vector2 basePointPreview;
	protected GameObject[,] previewArr;
	
	public void SetSize(int Size) { size = Size; }
	
	public float GetScale() { return scale; }
	
	public void SetScale(float Scale) { scale = Scale; /*Debug.Log("Scale: " + scale);*/ }
	
	public int GetSize() { return size; }
	
	public Vector2Int GetBasePoint() { return new Vector2Int((int)basePoint.x, (int)basePoint.y); }
	
	public int GetWidth() { return array.GetLength(0); }
	public int GetHeight() { return array.GetLength(1); }
	protected int getWidth() { return previewArr.GetLength(0); }
	protected int getHeight() { return previewArr.GetLength(1); }
	
	public float GetDisp(float Time, float MoveTime) { return cubeSize * scale * Mathf.Min(1f, Time / MoveTime); }
	
	public bool IsEmpty(int x, int y)
	{
		if (x >= 0 && x < GetWidth()
			&& y >= 0 && y < GetHeight())
		{
			return null == array[x, y];
		}
		return true;
	}
	
	protected bool isEmpty(int x, int y)
	{
		if (x >= 0 && x < getWidth()
			&& y >= 0 && y < getHeight())
		{
			return null == previewArr[x, y];
		}
		return true;
	}
	
	// width of field
	public void SetFieldWidth(float Width) { fieldWidth = Width; }
	// field coordinates to 3d
	public Vector3 FieldTo3D(int i, int j)
	{
		float offsetX = ((fieldWidth - 1) * cubeSize) / 2f;
		return scale * new Vector3(i * cubeSize - offsetX, (j + 0.5f) * cubeSize, 0);
	}
	
	protected Vector3 getPreview3D(int i, int j)
	{
		float offsetX = ((fieldWidth - 1) * cubeSize) / 2f;
		float x = i - basePointPreview.x;
		float y = j - basePointPreview.y;
		return scale * new Vector3(x * cubeSize - offsetX - (cubeSize) * (size * 0.5f + 2), y * cubeSize + cubeSize * 12, 0);
	}
	
	public GameObject GetCell(int x, int y)
	{
		if (x >= 0 && x < getWidth()
			&& y >= 0 && y < getHeight())
		{
			return array[x, y];
		}
		return null;
	}
	
	public void ClearCell(int x, int y)
	{
		if (x >= 0 && x < getWidth()
			&& y >= 0 && y < getHeight())
		{
			array[x, y] = null;
		}
	}
	
	private bool setCell(int x, int y, GameObject gameObj)
	{
		if (x >= 0 && x < getWidth()
			&& y >= 0 && y < getHeight())
		{
			previewArr[x, y] = gameObj;
			return true;
		}
		return false;
	}
	
	public void SetCellPos(int x, int y, Vector3 pos)
	{
		if (!IsEmpty(x, y))
		{
			array[x, y].transform.position = pos;
		}
	}
	
	protected int round(float x) { return (int)(x); }
	
	public void Mirror()
	{
		bool initialized = false;
		Vector2Int ul = new Vector2Int(0, 0);	// upper left
		Vector2Int lr = new Vector2Int(0, 0);	// lower right
		for (int j = 0; j < GetHeight(); j++)
		{
			for (int i = 0; i < GetWidth(); i++)
			{
				if (!IsEmpty(i, j))
				{
					if (initialized)
					{
						ul.x = Mathf.Min(ul.x, i);
						ul.y = Mathf.Min(ul.y, j);
						lr.x = Mathf.Max(lr.x, i);
						lr.y = Mathf.Max(lr.y, j);
					}
					else
					{
						ul = new Vector2Int(i, j);
						lr = new Vector2Int(i, j);
						initialized = true;
					}
				}
			}
		}
		Vector2 bp = 0.5f * new Vector2(ul.x + lr.x, ul.y + lr.y);
// 		Debug.Log("Mirror:" + bp);//.x + ", " + bp.y);
		prevArray = array;
		GameObject[,] newArray = new GameObject[GetWidth(), GetHeight()];
		for (int j = 0; j < GetHeight(); j++)
		{
			for (int i = 0; i < GetWidth(); i++)
			{
				if (!IsEmpty(i, j))
				{
					int newI = round(2 * bp.x - i);
					int newJ = j;
					newArray[newI, newJ] = array[i, j];
// 					Debug.Log("Mirror: (" + i + ", " + j + ") -> (" + newI + ", " + newJ + ")");
				}
			}
		}
		array = newArray;
	}
	
	public void Rotate()
	{
// 		Debug.Log("Rotate:" + basePoint);//.x + ", " + basePoint.y
		prevArray = array;
		GameObject[,] newArray = new GameObject[GetHeight(), GetWidth()];
		for (int j = 0; j < GetHeight(); j++)
		{
			for (int i = 0; i < GetWidth(); i++)
			{
				if (!IsEmpty(i, j))
				{
					float ni = basePoint.x - (j - basePoint.y);
					float nj = basePoint.y + (i - basePoint.x);
// 					Debug.Log("Rotate: (" + new Vector2(i, j) + " -> " + new Vector2(ni, nj));
					newArray[round(ni), round(nj)] = array[i, j];
				}
			}
		}
		array = newArray;
	}
	
	public void Restore()
	{
		array = prevArray;
	}
	
	protected void clear()
	{
		previewArr = new GameObject[2 * GetSize(), 2 * GetSize()];
	}
	
	// generate new n-tamino
	public void Generate()
	{
		array = previewArr;
		basePoint = basePointPreview;
		clear();
		
		int x = GetSize();
		int y = GetSize();
		
		int index = Random.Range(0, prefabArr.Length);
		int count = Random.Range(5 - 1, GetSize());
		
		setCell(x, y, Instantiate(prefabArr[index], new Vector3(0, 0, 0), Quaternion.identity, transform));
		Vector2Int ul = new Vector2Int(x, y);	// upper left
		Vector2Int lr = new Vector2Int(x, y);	// lower right

		do
		{
			int dir = Random.Range(0, 4);
			switch (dir)
			{
				case 0:
					if (x > 0) { x --; }
					break;
				case 1:
					if (y > 0) { y --; }
					break;
				case 2:
					if (x + 1 < GetSize()) { x ++; }
					break;
				case 3:
					if (y + 1 < GetSize()) { y ++; }
					break;
				default:
					break;
			}
			
			if (isEmpty(x, y))
			{
				setCell(x, y, Instantiate(prefabArr[index], new Vector3(0, 0, 0), Quaternion.identity, transform));
				
				ul.x = Mathf.Min(ul.x, x);
				ul.y = Mathf.Min(ul.y, y);
				lr.x = Mathf.Max(lr.x, x);
				lr.y = Mathf.Max(lr.y, y);
				count --;
			}
		}
		while (count > 0);
		
		basePointPreview = 0.5f * new Vector2(ul.x + lr.x, ul.y + lr.y);
		
		for (int j = 0; j < getHeight(); j++)
		{
			for (int i = 0; i < getWidth(); i++)
			{
				if (!isEmpty(i, j))
				{
					previewArr[i, j].transform.position = getPreview3D(i, j);
				}
			}
		}
	}
	
	// Start is called before the first frame update
	void Start()
	{
		clear();
	}

	// Update is called once per frame
	void Update()
	{
	}
}

