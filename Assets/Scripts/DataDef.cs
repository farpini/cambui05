using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public class WaypointsData : ScriptableObject
{
    public List<int[]> waypointsInRangeList;
    public List<int[]> desksWaypointsInRangeList;
    public List<int[]> waypointsDesksInRangeList;

    public void Initialize ()
    {
        waypointsInRangeList = new List<int[]>
        {
            new int[] { 1, 2, 7 }, //0
            new int[] { 0, 2, 3 }, //1
            new int[] { 0, 1, 4 }, //2
            new int[] { 1, 5 }, //3
            new int[] { 2, 6 }, //4
            new int[] { 3 }, //5
            new int[] { 4 }, //6
            new int[] { 0 }, //7
        };

        desksWaypointsInRangeList = new List<int[]>
        {
            new int[] { 1, 3}, //0
            new int[] { 3 }, //1
            new int[] { 5 }, //2
            new int[] { 5 }, //3
            new int[] { 1, 3}, //4
            new int[] { 3 }, //5
            new int[] { 5 }, //6
            new int[] { 5 }, //7
            new int[] { 2, 4 }, //8
            new int[] { 4 }, //9
            new int[] { 6 }, //10
            new int[] { 6 }, //11
            new int[] { 2, 4 }, //12
            new int[] { 4 }, //13
            new int[] { 6 }, //14
            new int[] { 6 }, //15
        };

        waypointsDesksInRangeList = new List<int[]>
        {
            new int[] { 0, 4},
            new int[] { 8, 12},
            new int[] { 0, 1, 4, 5},
            new int[] { 8, 9, 12, 13},
            new int[] { 2, 3, 6, 7},
            new int[] { 10, 11, 14, 15},
        };
    }
}

[Serializable]
public class UserRegisterData
{
    public string username;
    public string matricula;
    public string genero;
    public string tipo;

    public bool IsProfessor => (tipo == "professor");

    public UserRegisterData (string _u, string _m, string _g, string _t)
    {
        username = _u;
        matricula = _m;
        genero = _g;
        tipo = _t;
    }
}

[Serializable]
public class UserRuntimeData
{
    public string waypoint;
    public string roomId;
    public string state;
    public string message;
    public string epiId;
    public string fireState;

    public UserRuntimeData(int _w, int _rid, ClientState _cs, string _message, int _epiId, int _fireState)
    {
        waypoint = _w.ToString();
        roomId = _rid.ToString();
        state = _cs.ToString();
        message = _message;
        epiId = _epiId.ToString();
        fireState = _fireState.ToString();
    }
}

public enum ClientState
{
    Idle = 0,
    Walking = 1,
    Sit = 2
}

public enum UserRegisterAttribute
{
    username, matricula, genero, tipo
}

public enum UserRuntimeAttribute
{
    waypoint, roomId, state, message, epiId, fireState
}

public enum ClientGender
{
    none, masculino, feminino
}

public enum ClientType
{
    professor, aluno
}

public enum ClientStatus
{
    offline, online
}

public enum WorldState
{
    WaitingOnClassRoom = 0, ClassStarted = 1, WaitingOnPracticeRoom = 2, PracticeStarted = 3, FireAccident = 4, QuizStarted = 5, QuizFinished = 6
}

// WaitingOnClassRoom
// 0- "Faça o login para acesso as aulas"
// 1- "Dirige-se para a sala de aula"
// 2- "Aguarde o professor começar a aula"

// ClassStarted
// 0- "A aula começou"
// 1- "...."
// 2- "...."
// n- "Dirige-se para a sala prática"


// bool msgToStudent
// string msg = ""
// float msgDuration


[Serializable]
public class StateData 
{
    public string stateMsg;
    public bool stateMsgToShow;
    public float stateMsgDuration;
}

[Serializable]
public class StudentQuizData
{
    public string[] epiIds;
}

[Serializable]
public class WorldSettings
{
    public float characterSpeed;
    public int[] quizAnswerKeys;
}

public static class ProfessorInstructionsData
{
    public static Dictionary<int2, string> instructionsDict;

    public static void Initialize (int classSlidesCount)
    {
        instructionsDict = new();

        instructionsDict.Add(new int2((int)WorldState.WaitingOnClassRoom, 0),
            "Aguardando alunos entrarem na sala de aula. Quando pronto, clique Play para iniciar a aula.");

        for (int i = 0; i < classSlidesCount; i++)
        {
            instructionsDict.Add(new int2((int)WorldState.ClassStarted, i),
                "Slide: " + (i + 1).ToString() + " de " + classSlidesCount.ToString() + 
                "\nAvançar slide: botão da direita.\nRetornar slide: botão da esquerda" +
                ((i == classSlidesCount - 1) ? "\nEncerrar aula: botão play" : ""));
        }

        instructionsDict.Add(new int2((int)WorldState.WaitingOnPracticeRoom, 0),
            "Aguardando alunos entrarem na sala prática. Quando pronto, clique Play para iniciar a aula.");

        instructionsDict.Add(new int2((int)WorldState.PracticeStarted, 0),
            "Instrua os alunos a manusear os EPIs das suas bancadas.\nPara iniciar acidente de trabalho (incêndio), clique Play.");

        instructionsDict.Add(new int2((int)WorldState.FireAccident, 0),
            "Aguarde com que cada aluno apague uma parte do fogo até ele ser extinto por completo.\nCaso um aluno não conseguir usar o extintor, você deve apagá-lo.\n" +
            "Quando o fogo for extinto, clique Play para começar a etapa do quiz.");
        instructionsDict.Add(new int2((int)WorldState.FireAccident, 1),
            "Aguarde com que cada aluno apague uma parte do fogo até ele ser extinto por completo.\nCaso um aluno não conseguir usar o extintor, você deve apagá-lo.\n" +
            "Quando o fogo for extinto, clique Play para começar a etapa do quiz.");
        instructionsDict.Add(new int2((int)WorldState.FireAccident, 2),
            "Aguarde com que cada aluno apague uma parte do fogo até ele ser extinto por completo.\nCaso um aluno não conseguir usar o extintor, você deve apagá-lo.\n" +
            "Quando o fogo for extinto, clique Play para começar a etapa do quiz.");

        instructionsDict.Add(new int2((int)WorldState.QuizStarted, 0),
           "Instrua os alunos a trazerem os EPIs referentes às perguntas do quiz, das suas bancadas até a bancada central." +
           "\nPara iniciar as perguntas do quiz, clique botão da esquerda.");
        instructionsDict.Add(new int2((int)WorldState.QuizStarted, 1),
           "Estipule um tempo para que os alunos tragam o EPI até a bancada central como forma de resposta a pergunta do quiz." +
           "\nQuando achar apropriado, clique botão esquerda para a próxima pergunta.");
        instructionsDict.Add(new int2((int)WorldState.QuizStarted, 2),
           "Estipule um tempo para que os alunos tragam o EPI até a bancada central como forma de resposta a pergunta do quiz." +
           "\nQuando achar apropriado, clique botão da esquerda para a próxima pergunta.");
        instructionsDict.Add(new int2((int)WorldState.QuizStarted, 3),
           "Estipule um tempo para que os alunos tragam o EPI até a bancada central como forma de resposta a pergunta do quiz." +
           "\nQuando achar apropriado, clique botão da esquerda para a próxima pergunta.");
        instructionsDict.Add(new int2((int)WorldState.QuizStarted, 4),
           "Fim do quiz.");

        instructionsDict.Add(new int2((int)WorldState.QuizFinished, 0),
           "Fim do quiz e encerramento das aulas. Os resultados do quiz estão no quadro.");
    }

    public static string GetInstructionText (WorldState state, int stateArg)
    {
        var dictKey = new int2((int)state, stateArg);

        if (instructionsDict.TryGetValue(dictKey, out var dictValue))
        {
            return dictValue;
        }

        return "Sem instruções";
    }
}