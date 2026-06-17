# HomeFlow — User Guide

> Keep your home running smoothly.

---

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [Dashboard](#2-dashboard)
3. [Tasks](#3-tasks)
4. [Recurring Tasks](#4-recurring-tasks)
5. [Profile](#5-profile)
6. [FAQ](#6-faq)

---

## 1. Getting Started

How to sign in and take your first look around HomeFlow.

### Logging in

1. Open the HomeFlow app in your browser — you'll land on the Login page.
2. Enter your `username` and `password`, then click **Sign In**.
3. On success you're redirected to the Dashboard automatically.

> **Tip:** The app ships with two demo accounts — `pedro` and `maria` — both using the password `Password123!`. Use these to explore before adding real household members.

### First look

1. The sidebar lets you move between Dashboard, Tasks, Recurring Tasks, and Profile.
2. The Dashboard gives you a snapshot of what's happening — overdue items, today's tasks, and work distribution across household members.
3. Start on the Dashboard, then head to **Tasks** to create your first task.

> **Note:** Sessions expire after a period of inactivity. If you're redirected to Login unexpectedly, just sign in again.

---

## 2. Dashboard

At-a-glance view of the whole household's task situation.

### Stat cards

| Card | What it shows |
|------|--------------|
| **Overdue** | Tasks past their due date that are still not completed. Keep this at zero. |
| **Due today** | Tasks with today's date, regardless of status. |
| **Pending** | Tasks not yet started. |
| **Completed** | All tasks ever marked done. |

> **Tip:** Stat cards are read-only. Head to the Tasks page to act on items.

### Member distribution

1. Shows each household member alongside their **active task count** — tasks not yet completed.
2. Use it to spot imbalances and reassign if needed.

### Today's tasks

1. Lists tasks due today. If empty, it shows "Nothing due today."
2. To act on a task, navigate to the **Tasks** page and find it there.

---

## 3. Tasks

Create, manage, and complete one-off household tasks.

### Creating a task

1. Click **New Task** at the top of the Tasks page.
2. Enter a **title** (required) and an optional description.
3. Pick an **assignee** from the dropdown.
4. Set a **due date** if the task has a deadline.
5. Click **Save** — the task appears in the list immediately.

### Filtering

| Filter | Options |
|--------|---------|
| **Assignee** | Any household member |
| **Status** | `Pending` · `InProgress` · `Completed` · `Overdue` |
| **Type** | `OneOff` · `Recurring` |

> **Tip:** Filters stack — combine Assignee + Status to see exactly "Maria's overdue tasks" at once.

### Complete, edit, delete

1. **Complete** — click the checkmark on the task card. Status changes to `Completed`.
2. **Edit** — click the pencil icon; the form opens pre-filled. Save when done.
3. **Delete** — click the trash icon; a confirmation prompt appears before deletion.

> **Note:** Deletion is permanent. If you only want to stop working on a task, mark it `Completed` instead.

---

## 4. Recurring Tasks

Define a chore template once and generate individual tasks from it, rotating automatically through household members.

### What is a template?

1. A template describes a repeating chore — its title, frequency, and who takes turns doing it.
2. It doesn't create tasks on its own; you generate the next task manually when needed.
3. Generated tasks appear on the Tasks page exactly like any other task.

### Creating a template

1. Click **New Template** on the Recurring Tasks page.
2. Enter a **title** for the chore (e.g. "Take out the bins").
3. Set the **frequency** — number of days between occurrences.
4. In the **Rotation Order** section, add household members in the order they'll take turns.
5. Click **Save Template**.

> **Tip:** You can add the same person more than once in the rotation to weight the schedule — useful if someone is home more often.

### Generating the next task

1. Find the template in the list and click **Generate Next Task**.
2. HomeFlow assigns it to the next person in the rotation and sets the due date based on the template's frequency.
3. The task appears immediately on the Tasks page.

> **Note:** The rotation advances each time a task is *generated*, not each time one is completed. Generate the next task only when the previous one is done.

---

## 5. Profile

View your account details and sign out of HomeFlow.

### Account info

1. Navigate to **Profile** in the sidebar.
2. You'll see your **display name**, **username**, and **email address**.
3. These are set at account creation and can only be changed by an administrator.

### Signing out

1. Click **Sign Out** on the Profile page.
2. Your session ends immediately and you're returned to the Login page.

> **Tip:** Always sign out on shared devices so other household members can't access your session.

---

## 6. FAQ

**How do I mark a task as complete?**

Go to the Tasks page and find the task you've finished. Click the **checkmark button** on the task card. The status updates to `Completed` immediately and it stops appearing in overdue or pending counts.

---

**What happens when a recurring task is generated?**

Clicking **Generate Next Task** creates a new one-off task assigned to the next person in the rotation, with a due date calculated from the template's frequency. The task then lives on the Tasks page — you can edit, complete, or delete it like any other.

---

**Who can complete a task?**

Any logged-in household member can complete any task — not just the assignee. If someone else finishes a chore, they can mark it done without requiring the assigned person to log in.

---

**What does "Overdue" mean?**

A task is overdue when its due date has passed and its status is still `Pending` or `InProgress`. This is calculated automatically — you don't update it manually. Once you complete the task it disappears from the overdue count.

---

**How does the rotation work?**

Each template stores an ordered list of household members. When you generate the next task, HomeFlow assigns it to whoever is next in the list and advances the position by one. At the end of the list it wraps back to the start — so a rotation of Pedro → Maria alternates forever.
