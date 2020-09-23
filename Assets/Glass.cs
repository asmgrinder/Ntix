using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Glass : MonoBehaviour
{
	protected enum MoveState { NoMove, Fall, MoveLeft, MoveRight, LineRemove };
	protected enum KEY { KEY_TAB, KEY_UP, KEY_DOWN, KEY_LEFT, KEY_RIGHT, KEY_COUNT };
	
	public float delayToFall = 0.5f;
	public float delayToMirror = 0.25f;
	public float delayToRotate = 0.15f;
	public float moveTime = 0.1f;
	public GameObject figureGameObject;
	public GameObject scoreGameObject;
	
	protected Vector2Int[] fieldSizes = { new Vector2Int(22, 22), new Vector2Int(27, 27), new Vector2Int(33, 33) };
	
	protected int fieldSizeX = 20;
	protected int fieldSizeY = 20;
	
	protected const float baseFieldX = 10f;
	protected const float baseFieldY = 10f;
	protected Vector2Int figurePos;
	
	protected MoveState moveState = MoveState.NoMove;
	
	protected int lineToRemove = -1;
	
	protected GameObject[,] fieldArr;
	
	protected float time = 0;
	protected float timeToFall = 0;
	protected float timeToRotate = 0;
	protected float timeToMirror = 0;
	
	protected float keyboardDelay = 0.1f;
	protected float keyboardTime = 0;
	
	protected bool paused = false;
	
	protected const int scoreWeight = 10; 
	protected int score = 0;
	
	protected Figure figure;
	
	protected bool[] keyStates = new bool[(int)KEY.KEY_COUNT];
	
	
	protected void addScore(int Add)
	{
		score += Add * scoreWeight;
		TextMeshProUGUI scoretmp = scoreGameObject.GetComponent<TextMeshProUGUI>();
		scoretmp.SetText("Score: " + score);
	}
	
	protected int getWidth() { return fieldArr.GetLength(0); }
	
	protected int getHeight() { return fieldArr.GetLength(1); }
	
	protected bool isEmpty(int x, int y)
	{
		if (x >= 0 && x < getWidth() && y >= 0)
		{
			if (y < getHeight())
			{
				return null == fieldArr[x, y];
			}
			return true;
		}
		return false;
	}
	
	// test if figure overlaps field
	protected bool testBump(int x, int y)
	{
		for (int j = 0; j < figure.GetHeight(); j++)
		{
			for (int i = 0; i < figure.GetWidth(); i++)
			{
				if (!isEmpty(x + i, y + j)
					&& !figure.IsEmpty(i, j))
				{
					return true;
				}
			}
		}
		return false;
	}
	
	protected void setFigurePos()
	{
		for (int j = 0; j < figure.GetHeight(); j++)
		{
			for (int i = 0; i < figure.GetWidth(); i++)
			{
				if (!figure.IsEmpty(i, j))
				{
					Vector3 cellpos = figure.FieldTo3D(i + figurePos.x, j + figurePos.y);
					float disp = figure.GetDisp(time, moveTime);
					switch (moveState)
					{
						case MoveState.Fall:
							cellpos.y -= disp;
							break;
						case MoveState.MoveLeft:
							cellpos.x -= disp;
							break;
						case MoveState.MoveRight:
							cellpos.x += disp;
							break;
					}
					figure.SetCellPos(i, j, cellpos);
				}
			}
		}
	}
	
	protected void setFieldPos()
	{
		for (int j = lineToRemove; j < getHeight(); j++)
		{
			for (int i = 0; i < getWidth(); i++)
			{
				if (!isEmpty(i, j))
				{
					Vector3 cellpos = figure.FieldTo3D(i, j);
					float disp = figure.GetDisp(time, moveTime);
					if (j == lineToRemove)
					{
						disp *= 0.5f;
						float scale = 1f - time / moveTime;
						fieldArr[i, lineToRemove].transform.localScale = new Vector3(1f, scale, 1f);//SetActive(false);
					}
					switch (moveState)
					{
						case MoveState.LineRemove:
							cellpos.y -= disp;
							break;
					}
					fieldArr[i, j].transform.position = cellpos;
				}
			}
		}
	}
	
	protected void setFigurePos(int x, int y)
	{
		figurePos = new Vector2Int(x, y);
		setFigurePos();
	}
	
	protected void clear()
	{
		GameObject[] figures = GameObject.FindGameObjectsWithTag("Figure");
		foreach (GameObject figure in figures)
		{
			foreach (Transform child in figure.gameObject.transform)
			{
				Destroy(child.gameObject);
			}
		}
		score = 0;
		fieldArr = new GameObject[fieldSizeX, fieldSizeY];
		figure.SetFieldWidth(getWidth());
		figure.SetScale(Mathf.Min(baseFieldX / fieldSizeX, baseFieldY / fieldSizeY));
		transform.localScale = figure.GetScale() * new Vector3(1, 1, 1);
		time = 0;
		timeToFall = 0;
		moveState = MoveState.NoMove;
		figure.Generate();	// figure
		figure.Generate();	// preview
		Vector2Int basePoint = figure.GetBasePoint();
		setFigurePos(getWidth() / 2 - basePoint.x, getHeight() - basePoint.y);
	}
	
	protected void setCellPos(int x, int y)
	{
		if (!isEmpty(x, y))
		{
			Vector3 cellpos = figure.FieldTo3D(x, y);
			fieldArr[x, y].transform.position = cellpos;
		}
	}
	
	protected int findLineToRemove()
	{
		int line = -1;
		for (int j = 0; j < getHeight() && line < 0; j++)
		{
			line = j;	// check if line have no empty cells
			for (int i = 0; i < getWidth(); i++)
			{
				if (isEmpty(i, j))
				{
					line = -1;
					break;
				}
			}
		}
		return line;
	}
	
	protected void removeLine()
	{
		if (lineToRemove >= 0)	// if line have no empty cells move rest of the field down
		{
			addScore(getWidth());
			for (int i = 0; i < getWidth(); i++)
			{
				Destroy(fieldArr[i, lineToRemove]);
			}
			for (int j = lineToRemove; j + 1 < getHeight(); j++)
			{
				for (int i = 0; i < getWidth(); i++)
				{
					fieldArr[i, j] = fieldArr[i, j + 1];
					setCellPos(i, j);
				}
			}
			for (int i = 0; i < getWidth(); i++)
			{
				fieldArr[i, getHeight() - 1] = null;
			}
			lineToRemove = -1;
		}
	}
	
	protected bool copyFigureToField()
	{
		bool gameOver = false;
		for (int j = 0; j < figure.GetHeight(); j++)
		{
			for (int i = 0; i < figure.GetWidth(); i++)
			{
				if (!figure.IsEmpty(i, j))
				{
					int x = i + figurePos.x;
					int y = j + figurePos.y;
					if (x >= 0 && x < getWidth()
						&& y >= 0 && y < getHeight())
					{
						fieldArr[x, y] = figure.GetCell(i, j);
						figure.ClearCell(i, j);
						addScore(1);
					}
					else
					{
						gameOver = true;
					}
				}
			}
		}
		return gameOver;
	}
	
	protected void generateNextFigure()
	{
		figure.Generate();
		Vector2Int basePoint = figure.GetBasePoint();
		setFigurePos(getWidth() / 2 - basePoint.x, getHeight() - basePoint.y);
		timeToFall = delayToFall;
	}
	
	protected void onGameOver()
	{
		//SceneManager.LoadScene("Menu");
		clear();
	}
	
	protected void gameLogic()
	{
		keyboardTime += Time.deltaTime;
		if (keyboardTime > keyboardDelay
			&& (Input.GetKey(KeyCode.Pause)
				|| Input.GetKey(KeyCode.F1)))
		{
			keyboardTime = 0;
			paused = !paused;
		}
		if (paused)
		{
			return;
		}
		
		timeToFall -= Time.deltaTime;
		timeToRotate = Mathf.Max(0, timeToRotate - Time.deltaTime);
		timeToMirror = Mathf.Max(0, timeToMirror - Time.deltaTime);
		
		if (Input.GetKey(KeyCode.Escape))
		{
			SceneManager.LoadScene("Menu");
		}
		
		
		KeyCode[] keyNames = { KeyCode.Tab, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
		
		for (int i = 0; i < keyNames.Length; i++)
		{
			if (Input.GetKey(keyNames[i]))
			{
				keyStates[i] = true;
			}
		}
		
		switch (moveState)
		{
			case MoveState.LineRemove:
				if (lineToRemove >= 0)
				{	// there are lines to remove
					time += Time.deltaTime;
					setFieldPos();
					if (time >= moveTime)
					{
						time = 0;
						removeLine();
						lineToRemove = findLineToRemove();
					}
				}
				else// no more lines to remove
				{
					generateNextFigure();
					if (testBump(figurePos.x, figurePos.y))
					{
						onGameOver();
					}
					moveState = MoveState.NoMove;
				}
				break;
			case MoveState.NoMove:									// idle state
				time = 0;
				if (timeToFall <= 0)								// if time to fall, initiate fall
				{
					timeToFall = delayToFall;						// reset timer
					if (!testBump(figurePos.x, figurePos.y - 1))	// if fall possible
					{
						moveState = MoveState.Fall;					// change state
					}
					else
					{
						setFigurePos();
						bool gameOver = copyFigureToField();
						if (gameOver)
						{	// restart round
							onGameOver();
						}
						else// find line to remove
						{
							lineToRemove = findLineToRemove();
							moveState = MoveState.LineRemove;
							time = 0;
						}
					}
				}
				else												// check keyboard state
				{
					if (keyStates[(int)KEY.KEY_TAB])				//Input.GetKey(KeyCode.Tab))
					{
						keyStates[(int)KEY.KEY_TAB] = false;
						if (timeToMirror <= 0)
						{
							figure.Mirror();
							if (testBump(figurePos.x, figurePos.y))
							{
								figure.Restore();
							}
							else
							{
								timeToMirror = delayToMirror;
							}
						}
					}
					else if (keyStates[(int)KEY.KEY_UP])
					{
						keyStates[(int)KEY.KEY_UP] = false;
						if (timeToRotate <= 0)
						{
							figure.Rotate();
							if (testBump(figurePos.x, figurePos.y))
							{
								figure.Restore();
							}
							else
							{
								timeToRotate = delayToRotate;
							}
						}
					}
					else if (keyStates[(int)KEY.KEY_LEFT])
					{
						keyStates[(int)KEY.KEY_LEFT] = false;
						if (!testBump(figurePos.x - 1, figurePos.y))
						{
							moveState = MoveState.MoveLeft;
						}
					}
					else if (keyStates[(int)KEY.KEY_RIGHT])
					{
						keyStates[(int)KEY.KEY_RIGHT] = false;
						if (!testBump(figurePos.x + 1, figurePos.y))
						{
							moveState = MoveState.MoveRight;
						}
					}
					else if (keyStates[(int)KEY.KEY_DOWN])
					{
						keyStates[(int)KEY.KEY_DOWN] = false;
						if (!testBump(figurePos.x, figurePos.y - 1))
						{
							moveState = MoveState.Fall;
						}
					}
				}
				break;
			case MoveState.Fall:										// fall state
				time += Time.deltaTime;
				if (time > moveTime)
				{
					figurePos.y--;
					timeToFall = delayToFall;
					keyStates[(int)KEY.KEY_DOWN] = false;
					moveState = MoveState.NoMove;
				}
				break;
			case MoveState.MoveLeft:
				time += Time.deltaTime;
				if (time > moveTime)
				{
					figurePos.x--;
					keyStates[(int)KEY.KEY_LEFT] = false;
					moveState = MoveState.NoMove;
				}
				break;
			case MoveState.MoveRight:
				time += Time.deltaTime;
				if (time > moveTime)
				{
					figurePos.x++;
					keyStates[(int)KEY.KEY_RIGHT] = false;
					moveState = MoveState.NoMove;
				}
				break;
		}
		setFigurePos();
	}
	
	void Awake()
	{
		figure = figureGameObject.GetComponent<Figure>();
	}
	// Start is called before the first frame update
	void Start()
	{
		int gameType = PlayerPrefs.GetInt("game_type");
		switch (gameType)
		{
			case 6: case 7: case 8:
				figure.SetSize(gameType);
				fieldSizeX = fieldSizes[gameType - 6].x;
				fieldSizeY = fieldSizes[gameType - 6].y;
				break;
		}
		clear();
	}
	
	// Update is called once per frame
	void Update()
	{
		gameLogic();
	}
}
