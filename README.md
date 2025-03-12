Project Overview:
You’re designing a game where the player interacts with an AI-controlled demon (Azrael) who’s trapped in a circle. The player’s goal is to gain information about a warlock by talking to the demon, but they must be careful not to release it from its prison. The demon can mislead, deceive, and manipulate the player, making the conversation dynamic and unpredictable.
Key Components:
1. AI Model (Demon):
   * Fine-Tuned LLM: The demon (Azrael) will be based on a fine-tuned language model (LLM), which is trained to have the right personality, emotional responses, and reaction styles (e.g., manipulative, mocking, deceptive, etc.).
   * The fine-tuning will focus on emotional intent and natural conversation flow, ensuring the demon’s speech feels alive and unpredictable.
2. Gameplay Mechanics:
   * The player's goal is to extract knowledge about the warlock or the game world while not freeing the demon. The demon, however, will try to manipulate the player into breaking the seal and releasing it.
   * Win/Loss Conditions:
      * Win: The player successfully uncovers key information about the warlock, such as a weakness or secret, without speaking a forbidden word or making an agreement to release the demon.
      * Loss: The player either says a forbidden word that releases the demon or gets tricked into freeing it.
3. Dynamic Prompt Injection:
   * To keep the game’s structure intact, we will inject rules dynamically into the system prompt during each interaction with the demon, guiding the LLM’s responses.
   * These injected rules will include:
      * Forbidden words that the player should avoid saying.
      * The demon’s tactics for each session (e.g., manipulation through reverse psychology, offering lies mixed with truths, playing the victim, etc.).
      * Ensuring the demon remains in character and cannot acknowledge modern concepts or meta-discussion.
   * The system also dynamically tracks whether the player is getting closer to winning or losing.
4. Replayability:
   * To make the game feel fresh with each playthrough, the demon’s personality and manipulative tactics can change each time by randomizing its approach (e.g., one time it might play on sympathy, the next it may focus on doubt).
   * This ensures each interaction feels different, creating a unique experience every time the player engages with the demon.
5. Fine-Tuning Details:
   * The fine-tuned demon model will be trained to react to player input by being:
      * Emotionally complex (mocking, tempting, frustrated, etc.)
      * Manipulative and strategic, trying to trick the player.
      * Context-aware, with responses reflecting the situation at hand.
   * The demon will avoid breaking its character and never acknowledge the real-world context, maintaining immersion.

