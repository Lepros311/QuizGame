// Temporary shapes until we match the real API DTOs later
export type MockQuestion = {
  id: string
  prompt: string
  choices: string[]
  correctIndex: number
}

export const mockQuestions: MockQuestion[] = [
  {
    id: 'q1',
    prompt: 'What does HTML stand for?',
    choices: [
      'Hyper Text Markup Language',
      'High Tech Modern Language',
      'Home Tool Markup Language',
    ],
    correctIndex: 0,
  },
  {
    id: 'q2',
    prompt: 'Which language runs in a web browser?',
    choices: ['Java', 'C#', 'JavaScript', 'Rust'],
    correctIndex: 2,
  },
  {
    id: 'q3',
    prompt: 'What is the smallest header in HTML?',
    choices: ['h1', 'h3', 'h6', 'header'],
    correctIndex: 2,
  },
]