Sure! Here's a possible layout to structure the different components of your game, breaking it down into sections that cover the design, gameplay mechanics, and system interactions.

---

### **Game Layout: Interacting with Azrael, the Demon**

---

### **1. Introduction: The Setting**
- **Scene**: The player enters a dark, eerie chamber where Azrael, the demon, is trapped within a glowing, mystical circle. The atmosphere is tense, and the demon’s eyes gleam with malice as it senses the player's presence.
- **Narrative**: The player is tasked with gaining crucial information about a warlock without releasing Azrael from the circle. There’s an ancient lore about the demon’s past, and only by extracting knowledge from it will the player be able to uncover the secrets of the warlock.
- **Objective**: The player must converse with the demon to learn about the warlock, but the demon will manipulate the player, attempting to get them to make a fatal mistake (such as freeing it).

---

### **2. Main Gameplay Screen:**
   - **Visuals**:
      - **Demon (Azrael)**: The demon is visually depicted within the magic circle, exuding a presence of both menace and charm. It may have shifting features (e.g., subtle facial expression changes) to reflect its emotional manipulation.
      - **Conversation Interface**: A dynamic dialogue box that displays the conversation, with options for the player to choose from. The player can also input text for custom responses.
      - **Status Indicators**: 
        - **Information Progress**: A meter that shows how close the player is to uncovering key information about the warlock.
        - **Demon's Trust**: A subtle indicator that reflects how much the demon trusts the player, influencing its responses. If the player trusts the demon too much or says the wrong things, they risk triggering the loss condition.
        - **Forbidden Word Tracker**: This appears as a list of “forbidden words” that the player must avoid. These words could be gradually revealed based on the demon’s behavior.

---

### **3. AI Model (Demon):**
   - **Behavior Mechanics**:
     - The demon’s personality shifts each time the player engages with it. Some sessions may have the demon trying to manipulate through **reverse psychology**, while others may focus on **emotional manipulation** (guilt, fear, etc.).
     - The demon will have a dynamic script based on its current emotional state, the player's dialogue, and other influencing factors like how much the player has uncovered.
     - **Manipulation Styles**:
       - **Sympathy**: The demon may appeal to the player's compassion, claiming to be a victim and suggesting that releasing it would be an act of mercy.
       - **Mockery**: It could belittle the player’s intelligence or courage, challenging them to prove their worth.
       - **Doubt**: The demon might plant seeds of doubt about the player’s goal, suggesting the warlock isn't worth their time or that the player is missing important clues.
   
---

### **4. Gameplay Mechanics:**
   - **Player’s Goals**:
     - Extract information about the warlock’s weaknesses or secrets.
     - Avoid releasing the demon by avoiding the forbidden words or manipulative agreements.
   - **Forbidden Words**:
     - These words are dynamically injected into the conversation by the system, and if the player accidentally says them, they risk breaking the seal.
   - **Manipulative Responses**:
     - **Deceptive Information**: At times, the demon will give the player half-truths or outright lies to mislead them, complicating the process of obtaining the truth.
     - **Escalating Consequences**: If the player becomes too trusting or too antagonistic, the demon’s tactics may escalate, making the dialogue more intense.
   
---

### **5. Dynamic Prompt Injection System:**
   - **Prompt Rules**: Each time the player interacts with the demon, the following rules are injected:
     - **Forbidden Words**: Randomly chosen from a pre-defined list for each session.
     - **Tactical Approach**: The system selects the demon’s manipulation strategy (e.g., guilt trip, mockery, flattery).
     - **Game Progress**: Based on player choices, the game tracks how close the player is to uncovering the warlock’s secrets.
     - **Emotional Tone**: The demon’s emotional response will vary, and the LLM is adjusted to ensure it reacts accordingly (e.g., irritated if the player is evasive, pleased if they show weakness).

---

### **6. Replayability:**
   - **Randomized Demon Tactics**: Every playthrough offers a different emotional angle from the demon. The player might face a demon who uses a different manipulation style than before.
   - **Multiple Endings**:
     - **Good Ending**: The player successfully gathers the information and does not release the demon.
     - **Bad Ending**: The player is tricked into releasing the demon, or they say the forbidden word.
   - **Hints and Traps**: Each new playthrough can provide subtle clues or traps based on past mistakes, giving the player opportunities to approach the demon differently.
   
---

### **7. Fine-Tuning Details for the Demon’s AI Model**:
   - **Core Attributes**:
     - **Emotional Complexity**: The demon reacts to the player in ways that challenge their sense of morality or logic. It may offer kindness one moment, then mock or manipulate the next.
     - **Manipulative Intelligence**: The AI is trained to focus on subtle strategies to mislead the player, like planting doubt, playing to their emotions, or offering temptations.
     - **Context Awareness**: The demon can understand and react to the context of the conversation. For example, if the player tries to get too direct, it may attempt to change the topic.

---

### **8. User Interface and Interaction Design**:
   - **Dialogue Choices**: The player can choose from multiple dialogue options, but some options are risky (e.g., agreeing to free the demon, being too open, or saying something that might break the seal).
   - **Hints System**: Occasionally, a hint or warning will appear for the player if they are getting too close to making a fatal error, such as triggering a forbidden word or agreeing to release the demon.
   
---

This structure provides a layered approach to gameplay, where the player must carefully navigate both the demon's manipulations and the quest for information about the warlock. Each conversation becomes a high-stakes puzzle, with emotional and strategic risks, all guided by the AI’s unpredictable behavior.
