﻿using GhostRunner.Models;
using GhostRunner.SL;
using GhostRunner.Utils;
using GhostRunner.ViewModels.Main.Partials;
using GhostRunner.ViewModels.Scripts;
using GhostRunner.ViewModels.Scripts.Partials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GhostRunner.Controllers
{
    public class ScriptsController : Controller
    {
        private ProjectService _projectService;

        public ScriptsController()
        {
            _projectService = new ProjectService();
        }
        
        [NoCache]
        [Authenticate]
        public ActionResult Index(String id)
        {
            IndexModel indexModel = new IndexModel();
            
            indexModel.User = ((User)ViewData["User"]);
            indexModel.Project = _projectService.GetProject(id);
            indexModel.Scripts = _projectService.GetAllProjectScripts(indexModel.Project.ID);

            foreach (Script script in indexModel.Scripts)
            {
                if (!indexModel.ScriptTasks.ContainsKey(script.ExternalId)) indexModel.ScriptTasks.Add(script.ExternalId, new List<Task>());
            }

            return View(indexModel);
        }

        #region Create a new script
        
        [NoCache]
        [Authenticate]
        public ActionResult GetCreateScriptDialog(String projectId)
        {
            CreateScriptModel createScriptModel = new CreateScriptModel();
            createScriptModel.Project = _projectService.GetProject(projectId);
            createScriptModel.Script = new Script();

            return PartialView("Partials/CreateScript", createScriptModel);
        }
        
        [NoCache]
        [Authenticate]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InsertNewScript(String id, CreateScriptModel createScriptModel)
        {
            Script script = _projectService.InsertScript(id, createScriptModel.Script.Name, createScriptModel.Script.Description, createScriptModel.Script.Content);

            return RedirectToAction("Index/" + id, "Scripts");
        }

        #endregion

        #region Edit a current script

        [NoCache]
        [Authenticate]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetEditScriptDialog(String scriptId)
        {
            EditScriptModel editScriptModel = new EditScriptModel();
            editScriptModel.Script = _projectService.GetScript(scriptId);
            editScriptModel.User = ((User)ViewData["User"]);
            editScriptModel.Project = editScriptModel.Script.Project;

            return PartialView("Partials/EditScript", editScriptModel);
        }

        [NoCache]
        [Authenticate]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(String id, EditScriptModel editScriptModel)
        {
            _projectService.UpdateScript(id, editScriptModel.Script.Name, editScriptModel.Script.Description, editScriptModel.Script.Content);

            Script script = _projectService.GetScript(id);

            return RedirectToAction("Index/" + script.Project.ExternalId, "Scripts", new { view = "scripts" });
        }

        #endregion

        #region Delete a script

        [NoCache]
        [Authenticate]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ConfirmDeleteScript(String scriptId)
        {
            ConfirmDeleteScriptModel confirmDeleteModel = new ConfirmDeleteScriptModel();
            confirmDeleteModel.Script = _projectService.GetScript(scriptId);

            return PartialView("Partials/ConfirmDeleteScript", confirmDeleteModel);
        }

        [NoCache]
        [Authenticate]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteScript(String id, ConfirmDeleteScriptModel confirmDeleteScriptModel)
        {
            Script script = _projectService.GetScript(id);

            if (script != null)
            {
                String projectId = script.Project.ExternalId;

                _projectService.DeleteScript(id);

                return RedirectToAction("Index/" + projectId, "Scripts");
            }
            else
            {
                return RedirectToAction("Index", "Main");
            }
        }

        #endregion

        #region Create a new script task

        [NoCache]
        [Authenticate]
        public ActionResult GetRunScriptDialog(String scriptId)
        {
            RunScriptModel runScriptModel = new RunScriptModel();
            runScriptModel.User = ((User)ViewData["User"]);
            runScriptModel.Script = _projectService.GetScript(scriptId);
            runScriptModel.Project = runScriptModel.Script.Project;

            runScriptModel.Task = new Task();
            runScriptModel.Task.Name = runScriptModel.Script.Name;

            runScriptModel.TaskParameters = new List<TaskParameter>();

            foreach (String parameter in runScriptModel.Script.GetAllParameters())
            {
                TaskParameter taskParameter = new TaskParameter();
                taskParameter.Name = parameter;
                taskParameter.Value = String.Empty;

                runScriptModel.TaskParameters.Add(taskParameter);
            }

            return PartialView("Partials/RunScript", runScriptModel);
        }

        [NoCache]
        [Authenticate]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateTask(String id, RunScriptModel runScriptModel)
        {
            Script script = _projectService.GetScript(id);

            Task scriptTask = _projectService.InsertScriptTask(((User)ViewData["User"]).ID, id, runScriptModel.Task.Name, runScriptModel.TaskParameters);

            return RedirectToAction("Index/" + script.Project.ExternalId, "Scripts", new { view = "scripts" });
        }

        #endregion
    }
}