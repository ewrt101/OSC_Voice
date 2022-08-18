# OSC Voice

OSC Voice is a OSC based "voice" program for VRChat.

## currently is program has 4 modes:
1) outputs current time to the VRChat Chatbox every 1.5~ seconds
2) reads line by line from DisplayText.txt and outputs to VRChat Chatbox
3) local Speech to text and outputs to VRChat Chatbox
4) AssemblyAI Speech to text and outputs to VRChat Chatbox (AssemblyAI is a paid service)


## local SST
Local STT needs a model.tflite and a Vocabulary.scorer file to work you can get the needed files from https://coqui.ai/models, but they will need to be renamed

This might also only work on nivida GPUs but im not sure

You can exit this mode by saying "leave program" *if it works*

## AssemblyAI
For AssemblyAI you will need to create an account at assemblyai.com and place you key in the AssemblyAI.key folder for this to work, OSC_Voice uses realtime SST and this is a paid service at about $0.00025 per second

## ToDO
- Tidy up my shit storm of a code base and move the OSC and SST code to a DLL
- Add a UI
- Add google & azure SST for extra choices
- Add Avatar Voice Command for that cool anime vibe
