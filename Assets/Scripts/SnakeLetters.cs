using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System.Security.Cryptography;


public class SnakeLetters : Snake
{
    [SerializeField] private WordManager wordManager;
    [SerializeField] private GameObject floatingTextPrefab;

    protected override void ResetState(bool colorEffect = true)
    {
        if (startBool == false) {
        // also reset currentWord and bonusPoints (after scene initialisation)
        UpdateWords();
        }
        else {
            // don't run 'UpdateWords' at scene initialisation
            startBool = false;
        }

        base.ResetState(colorEffect);
    }

    protected override void Grow(Collider2D food)
    // add food's letter to current word when it is 'eaten'
    {
        base.Grow(food);
        this.GetComponent<AudioSource>().Play();

        if (food.gameObject.GetComponent<FoodLetter>() != null)
        {
            FoodLetter foodLetter = food.gameObject.GetComponent<FoodLetter>();
            wordManager.AddLetter(foodLetter);

            // Display letter in snake
            string letter = foodLetter.textMesh.text;
            segments[wordManager.currentWord.Length].gameObject.GetComponentInChildren<TextMesh>().text = letter;
        }
    }

    protected override Vector2Int GetInput()
    // 'Bank' current word if the user hits spacebar
    {
        if (Input.GetKeyDown(KeyCode.Space) && wordManager.currentWord != "") {
            UpdateWords();
        }

        return base.GetInput();
    }

    private void UpdateWords() 
    {
        for (int i = 0; i < wordManager.currentWord.Length + 1; i++) {
            // clear letters in snake
            segments[1 + i].gameObject.GetComponentInChildren<TextMesh>().text = "";
            // flash colours
            if (wordManager.InDictionary(wordManager.currentWord)) {
                colorManager.ColorPulseSuccess(segments[i].gameObject);
                }
            else {
                colorManager.ColorPulseFail(segments[i].gameObject);
            }
        }

        wordManager.BankWord();
        // add bonus points
        pointCounter += wordManager.bonusPoints;
        // show points
        if (wordManager.bonusPoints > 0) {
            ShowPoints(wordManager.bonusPoints);
        }

    }

    private void ShowPoints(int points)
    {
        if(floatingTextPrefab)
        {
            GameObject prefab = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);
            prefab.GetComponentInChildren<TextMesh>().text = $" + {points.ToString()} bonus!";
        }
    }

}
