# Frequently Asked Questions (FAQ)

Below are answers to common questions about FSM_API. For more details, follow the provided links to relevant User Guide sections.

---

### **Q: Is FSM_API compatible with my Unity version or platform?**

**A:**  
Yes. FSM_API is pure C#/.NET and works with all Unity versions that support .NET Framework 4.7.1 or higher. It is also compatible with any platform Unity supports.

---

### **Q: Can I use FSM_API outside of Unity?**

**A:**  
Absolutely! FSM_API is engine-agnostic and works in any C#/.NET application, including console, desktop, and server apps.  
See: [Getting Started - C#/.NET](03_Getting_Started_CSharp.md)

---

### **Q: How do I update an FSM definition at runtime?**

**A:**  
You can modify and rebuild FSM definitions using the `FSMBuilder`. Existing instances will adopt the new logic on their next update.  
See: [FSMBuilder Deep Dive](04_FSM_Builder_Deep_Dive.md)

---

### **Q: What's the difference between event-driven and frame-based FSMs?**

**A:**  
- **Event-driven FSMs** (`processRate = 0`) only update when you explicitly call for it—ideal for UI or turn-based logic.
- **Frame-based FSMs** (`processRate = -1` or `>0`) update every frame or every Nth frame—best for real-time gameplay.
See: [Performance Tips](08_Performance_Tips.md)

---

### **Q: My FSM isn't transitioning, what's wrong?**

**A:**  
Common causes include transition predicates always returning false, incorrect state names, or context data not updating.  
See: [Error Handling](07_Error_Handling.md)

---

### **Q: Can I nest FSMs?**

**A:**  
FSM_API does not directly support nested FSMs, but you can have a state manage another FSM instance via its context object.  
See: [Core Concepts](01_Core_Concepts.md)

---

### **Q: Why is RNG.cs included?**

**A:**  
`RNG.cs` provides fast, deterministic, and engine-agnostic random number generation for probabilistic FSM transitions, AI, and more.  
See: [RNG Utility](06_RNG_Utility.md)

---

### **Q: How do I handle errors in FSM_API?**

**A:**  
Subscribe to the `OnInternalApiError` event for centralized error logging and diagnostics.  
See: [Error Handling](07_Error_Handling.md)

---

### **Q: Can I use FSM_API with Unity's new Input System?**

**A:**  
Yes! FSM_API works with any input system. See the Pac-Man player example for integration tips.  
See: [Common Use Cases](09_Common_Use_Cases.md)

---

[Return to User Guide Index](_Index.md)