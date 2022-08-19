# OSC Voice

OSC Voice is a OSC based "voice" program for VRChat.

## currently is program has 4 modes:
1) outputs current time to the VRChat Chatbox every 1.5~ seconds
2) reads line by line from DisplayText.txt and outputs to VRChat Chatbox
3) local Speech to text and outputs to VRChat Chatbox
4) AssemblyAI Speech to text and outputs to VRChat Chatbox, this is running in realtime mode (AssemblyAI is a paid service)
4) AssemblyAI Speech to text and outputs to VRChat Chatbox, this is running in stream/chunk mode (AssemblyAI is a paid service)


## local SST
Local STT needs a model.tflite and a Vocabulary.scorer file to work you can get the needed files from https://coqui.ai/models, but they will need to be renamed

This might also only work on nivida GPUs but im not sure

You can exit this mode by saying "leave program" *(the enter key should work now too)*

## AssemblyAI
For AssemblyAI you will need to create an account at assemblyai.com and place you key in the AssemblyAI.key folder for this to work, OSC_Voice uses realtime SST and this is a paid service at about $0.00025 per second.

create an account here: https://www.assemblyai.com

- Realtime mode is the recomended mode but this does not turn off when not speeking, ***so costs can build up constantly***. its about $0.9 per hour
- stream/chunk mode only sends data when you are speaking so it will be much cheaper to run, the downside is if you speak for more than 15 seconds in one go it will give a error, its is a AssemblyAI enforced limit when using the /v2/stream endpoint vs the /realtime/ws endpoint


## ToDO
- Tidy up my shit storm of a code base and move the OSC and SST code to a DLL
- Add a UI
- Add google & azure SST for extra choices
- Add Avatar Voice Command for that cool anime vibe
