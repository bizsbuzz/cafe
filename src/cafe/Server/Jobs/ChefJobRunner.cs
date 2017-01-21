﻿using System;
using cafe.Shared;
using NLog;

namespace cafe.Server.Jobs
{
    public class ChefJobRunner
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(ChefJobRunner).FullName);

        private readonly JobRunner _runner;
        private readonly DownloadChefJob _downloadJob;
        private readonly InstallChefJob _installJob;
        private readonly RunChefJob _runChefJob;

        public ChefJobRunner(JobRunner runner, DownloadChefJob downloadJob, InstallChefJob installJob, RunChefJob runChefJob)
        {
            ListenForJobsToBeReady(downloadJob, installJob, runChefJob);
            _runner = runner;
            _downloadJob = downloadJob;
            _installJob = installJob;
            _runChefJob = runChefJob;
        }

        public DownloadChefJob DownloadChefJob => _downloadJob;

        public RunChefJob RunChefJob => _runChefJob;

        public InstallChefJob InstallChefJob => _installJob;

        private void ListenForJobsToBeReady(params Job[] jobs)
        {
            foreach (var job in jobs)
            {
                job.RunReady += JobRunReady;
            }
        }

        private void JobRunReady(object sender, JobRun jobRun)
        {
            Logger.Debug($"Adding {jobRun} to the queue to run");
            _runner.Enqueue(jobRun);
        }

        public ServerStatus ToStatus()
        {
            var status = _runner.ToStatus();
            status.ChefStatus = new ChefStatus()
            {
                IsRunning = RunChefJob.IsRunning,
                ExpectedNextRun = RunChefJob.RunPolicy.ExpectedNextRun?.ToDateTimeUtc(),
                Interval = RunChefJob.RunPolicy.Interval?.ToTimeSpan(),
                LastRun = RunChefJob.LastRun?.Start?.ToDateTimeUtc(),
                Version = InstallChefJob.CurrentVersion?.ToString()
            };
            return status;
        }

        public JobRunStatus FindStatusById(Guid id)
        {
            return _runner.FindStatusById(id);
        }
    }
}