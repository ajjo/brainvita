using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class TrainingData {
	public List<int> input;
	public List<int> output;
}

public class FileHelper {

	public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
	{
		TextWriter writer = null;
		try
		{
			var serializer = new XmlSerializer(typeof(T));
			writer = new StreamWriter(filePath, append);
			serializer.Serialize(writer, objectToWrite);
		}
		finally
		{
			if (writer != null)
				writer.Close();
		}
	}

	public static T ReadFromXmlFile<T>(string filePath) where T : new()
	{
		TextReader reader = null;
		try
		{
			var serializer = new XmlSerializer(typeof(T));
			reader = new StreamReader(filePath);
			return (T)serializer.Deserialize(reader);
		}
		finally
		{
			if (reader != null)
				reader.Close();
		}
	}
}

public class Game : MonoBehaviour {

	public Transform _Ball;
	private List<Ball> mBalls = new List<Ball>();
	private int mIndex = 1;
	private List<int> mInvalidList = new List<int>();
	private List<TrainingData> trainingData = new List<TrainingData>();

	// Use this for initialization
	void Start () {
		Init();
	}

	private void Init() {

		time_stamped = Time.realtimeSinceStartup;

		foreach(Ball b in mBalls) {
			GameObject.Destroy(b._t.gameObject);
		}

		mBalls.Clear();
		mInvalidList.Clear();
		mIndex = 1;
		
		for(int i=1;i<=121;i++) {
			
			if(i<27)
				mInvalidList.Add(i);
			if(i>29 && i<38)
				mInvalidList.Add(i);
			if(i>40 && i<47)
				mInvalidList.Add(i);
			if(i>53 && i<58)
				mInvalidList.Add(i);
			if(i>64 && i<69)
				mInvalidList.Add(i);
			if(i>75 && i<82)
				mInvalidList.Add(i);
			if(i>84 && i<93)
				mInvalidList.Add(i);
			if(i>95)
				mInvalidList.Add(i);
			
		}
		
		AddMarbles(4,11,-5,5);
		AddMarbles(3,11,-5,1);
		AddMarbles(4,11,-5,-2);
	}

	private void Reset() {

		Init();

	}

	private void AddMarbles(int row_count,int col_count, int x, int y) {

		int org_x = x;

		for(int i=0;i<row_count;i++)
		{
			for(int j=0;j<col_count;j++)
			{
				GameObject obj = (GameObject)GameObject.Instantiate(_Ball.gameObject);
				Transform obj_t = obj.transform;
				Vector3 pos = obj_t.position;
				TextMesh text = obj_t.GetComponentInChildren<TextMesh>();
				text.text = mIndex.ToString();

				Ball ball = obj.GetComponent<Ball>();
				ball._x = x;
				ball._y = y;
				ball._t = obj_t;
				ball._index = mIndex;
				ball._empty = false;

				// make thing invalid here...
				if(mIndex == 61) {
					ball.disappear();
				}
				else if(mInvalidList.Contains(mIndex)) {
					ball.disappear(true);
				}
			

				mIndex++;
				pos.x = x;
				pos.y = y;
				obj_t.position = pos;
				
				mBalls.Add(ball);
				x += 1;
			}

			y -= 1;
			x = org_x;
		}
	}

	private List<int> GetBoardData() {

		List<int> ids = new List<int>();

		foreach(Ball b in mBalls) {
			if(b.active()) 
				ids.Add(1);
			else
				ids.Add(0);
		}

		return ids;
	}

	private void printData(List<int> data) {

		Debug.Log ("Hellooo " + data.ToString());
		string str = "";
		foreach(int i in data) {
			str += i.ToString() +":";
		}

		Debug.Log (str);
	}

	private void Move() {

		TrainingData a = new TrainingData();
		a.input = GetBoardData();
		
		List<Ball> empty_balls = new List<Ball>();
		
		foreach(Ball b1 in mBalls) {
			if(b1._empty && !b1._invalid) {
				empty_balls.Add(b1);
			}
		}
				
		foreach(Ball b1 in empty_balls) {
			Play (b1);
		}

		int count = 1000;
		Ball b = null;

		Debug.Log ("-------------");

		for(int i=0;i<empty_balls.Count;i++) {
			Debug.Log ("Count for " + empty_balls[i]._index + " is " + empty_balls[i]._moves.Count);
			if(empty_balls[i]._moves.Count > 0 && empty_balls[i]._moves.Count < count) {
				count = empty_balls[i]._moves.Count;
				b = empty_balls[i];
			}
		}

		if(b != null) {
			List<Ball> similar_kinds = new List<Ball>();
			for(int i=0;i<empty_balls.Count;i++) {
				if(empty_balls[i]._moves.Count > 0 && empty_balls[i]._moves.Count == b._moves.Count) {
					similar_kinds.Add(empty_balls[i]);
				}
			}

			Debug.Log("Choosing from a randome of " + similar_kinds.Count);
			b = similar_kinds[Random.Range(0,similar_kinds.Count)];

			Debug.Log ("CHOOSING " + b._index);
				
			//Debug.Log ("Count = " + possible_moves.Count);
			if(b._moves.Count > 0) {
				Debug.Log ("Choosing...");
				Vector3 data = b._moves[b._moves.Count - 1];//Random.Range(0,b._moves.Count)];
				Debug.Log ("Chosen one = " + data.x + " : " + data.y);
				Ball moving_ball = GetBallFromIndex((int)data.x);
				Ball disappearing_ball = GetBallFromIndex((int)data.y);
				
				b.appear();
				moving_ball.disappear();
				disappearing_ball.disappear();					
			}
		}
	}

	private void Move1() {

		TrainingData a = new TrainingData();
		a.input = GetBoardData();

		List<Ball> empty_balls = new List<Ball>();

		foreach(Ball b in mBalls) {
			if(b._empty && !b._invalid) {
				empty_balls.Add(b);
			}
		}

		//empty_balls = ShuffleList<Ball>(empty_balls);

		SortList(ref empty_balls);

		bool move_done = false;

		while(empty_balls.Count > 0) {
			int index = 0;//Random.Range(0,empty_balls.Count);
			Ball b = empty_balls[index];
			move_done = Play1 (b);
			if(move_done)
				break;

			empty_balls.Remove(b);
		}

		a.output = GetBoardData();
		printData(a.input);
		printData(a.output);
		//trainingData.Add(a);

		//FileHelper.WriteToXmlFile<TrainingData>("/Users/deja/Marbles/Assets/training_data.xml",a,true);

		if(!move_done) {
			Debug.Log ("GAME OVER");
			//Reset ();
		}

	}

	private void SortList(ref List<Ball> inputList) {

		for(int i=0;i<inputList.Count-1;i++) {
			for(int j=i+1; j<inputList.Count;j++) {
				if(inputList[i]._y > inputList[j]._y) {
					Ball temp = inputList[i];
					inputList[i] = inputList[j];
					inputList[j] = temp;
				}
			}
		}
	}

	private List<E> ShuffleList<E>(List<E> inputList)
	{
		List<E> randomList = new List<E>();
		
		System.Random r = new System.Random();
		int randomIndex = 0;
		while (inputList.Count > 0)
		{
			randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
			randomList.Add(inputList[randomIndex]); //add it to the new, random list
			inputList.RemoveAt(randomIndex); //remove to avoid duplicates
		}
		
		return randomList; //return the new random list
	}

	private void Play(Ball chosen_ball) {

		int left_index = chosen_ball._index - 2;
		int right_index = chosen_ball._index + 2;
		int top_index = chosen_ball._index - 22;
		int bot_index = chosen_ball._index + 22;
		
		List<Vector3> possible_moves = new List<Vector3>();			
		
		Ball left_ball = GetBallFromIndex(left_index);
		Ball right_ball = GetBallFromIndex(right_index);
		Ball top_ball = GetBallFromIndex(top_index);
		Ball bot_ball = GetBallFromIndex(bot_index);
		
		Ball left_ball1 = GetBallFromIndex(left_index + 1);
		Ball right_ball1 = GetBallFromIndex(right_index - 1);
		Ball top_ball1 = GetBallFromIndex(top_index + 11);
		Ball bot_ball1 = GetBallFromIndex(bot_index - 11);

		if(mInvalidList.Contains(left_index) == false && !left_ball._empty && !left_ball1._empty) {
			Vector3 data = new Vector3(left_index, chosen_ball._index - 1, 1);	
			possible_moves.Add(data);
		}
		if(mInvalidList.Contains(right_index) == false && !right_ball._empty && !right_ball1._empty) {
			Vector3 data = new Vector3(right_index, chosen_ball._index + 1, 2);	
			possible_moves.Add(data);
		}
		if(mInvalidList.Contains(top_index) == false && !top_ball._empty && !top_ball1._empty) {
			Vector3 data = new Vector3(top_index, chosen_ball._index - 11, 3);	
			possible_moves.Add(data);
		}
		if(mInvalidList.Contains(bot_index) == false && !bot_ball._empty && !bot_ball1._empty) {
			Vector3 data = new Vector3(bot_index, chosen_ball._index + 11, 4);	
			possible_moves.Add(data);
		}

		//Debug.Log ("Count for index " + chosen_ball._index + " = " + possible_moves.Count);
		chosen_ball._moves = possible_moves;
	}
	
	private bool Play1(Ball chosen_ball) {

			int left_index = chosen_ball._index - 2;
			int right_index = chosen_ball._index + 2;
			int top_index = chosen_ball._index - 22;
			int bot_index = chosen_ball._index + 22;
			
			List<Vector3> possible_moves = new List<Vector3>();			
			
			Ball left_ball = GetBallFromIndex(left_index);
			Ball right_ball = GetBallFromIndex(right_index);
			Ball top_ball = GetBallFromIndex(top_index);
			Ball bot_ball = GetBallFromIndex(bot_index);

			Ball left_ball1 = GetBallFromIndex(left_index + 1);
			Ball right_ball1 = GetBallFromIndex(right_index - 1);
			Ball top_ball1 = GetBallFromIndex(top_index + 11);
			Ball bot_ball1 = GetBallFromIndex(bot_index - 11);

			if(mInvalidList.Contains(left_index) == false && !left_ball._empty && !left_ball1._empty) {
				Vector3 data = new Vector3(left_index, chosen_ball._index - 1, 1);	
				Debug.Log("Left = " + left_index + " : " + (chosen_ball._index - 1));
				possible_moves.Add(data);
			}
			if(mInvalidList.Contains(right_index) == false && !right_ball._empty && !right_ball1._empty) {
				Vector3 data = new Vector3(right_index, chosen_ball._index + 1, 2);	
				Debug.Log("Right = " + right_index + " : " + (chosen_ball._index + 1));
				possible_moves.Add(data);
			}
			if(mInvalidList.Contains(top_index) == false && !top_ball._empty && !top_ball1._empty) {
				Vector3 data = new Vector3(top_index, chosen_ball._index - 11, 3);	
				Debug.Log("Top = " + top_index + " : " + (chosen_ball._index - 11));
				possible_moves.Add(data);
			}
			if(mInvalidList.Contains(bot_index) == false && !bot_ball._empty && !bot_ball1._empty) {
				Vector3 data = new Vector3(bot_index, chosen_ball._index + 11, 4);	
				Debug.Log("Bottom = " + bot_index + " : " + (chosen_ball._index + 11));
				possible_moves.Add(data);
			}
			
			//Debug.Log ("Count = " + possible_moves.Count);
			if(possible_moves.Count > 0) {
				Debug.Log ("Choosing...");
				Vector3 data = possible_moves[Random.Range(0,possible_moves.Count)];
				Debug.Log ("Chosen one = " + data.x + " : " + data.y);
				Ball moving_ball = GetBallFromIndex((int)data.x);
				Ball disappearing_ball = GetBallFromIndex((int)data.y);
				
				chosen_ball.appear();
				moving_ball.disappear();
				disappearing_ball.disappear();					
				return true;
			}

			return false;
//		}
	}





	private Ball GetBallFromIndex(int index) {

		foreach(Ball b in mBalls) {
			if(b._index == index)
				return b;
		}

		return null;
	}

	float time_stamped = 0.0f;
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyUp(KeyCode.T)) {
			Move();
		}

		if((Time.realtimeSinceStartup - time_stamped) >= 0.5f) {
			time_stamped = Time.realtimeSinceStartup;
			Move ();
		}
	}
}
