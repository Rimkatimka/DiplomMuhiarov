using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public AuditRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<AuditLogDto> GetAll()
        {
            return _context.AuditLog
                .OrderByDescending(a => a.ActionTime)
                .Take(1000)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                })
                .ToList();
        }

        public List<AuditLogDto> GetByDate(DateTime from, DateTime to)
        {
            return _context.AuditLog
                .Where(a => a.ActionTime >= from && a.ActionTime <= to)
                .OrderByDescending(a => a.ActionTime)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                })
                .ToList();
        }

        public List<AuditLogDto> GetByUser(int userId)
        {
            return _context.AuditLog
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ActionTime)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                })
                .ToList();
        }

        public List<AuditLogDto> GetByTable(string tableName)
        {
            return _context.AuditLog
                .Where(a => a.TableName == tableName)
                .OrderByDescending(a => a.ActionTime)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                })
                .ToList();
        }

        public void Log(AuditLogDto dto)
        {
            var entity = new AuditLog
            {
                UserId = dto.UserId,
                ActionTime = DateTime.Now,
                ActionType = dto.ActionType,
                TableName = dto.TableName,
                RecordId = dto.RecordId,
                OldValuesJson = dto.Details,
                NewValuesJson = dto.Details
            };

            _context.AuditLog.Add(entity);
            _context.SaveChanges();
        }

        private string GetDetails(AuditLog log)
        {
            if (!string.IsNullOrEmpty(log.OldValuesJson) && !string.IsNullOrEmpty(log.NewValuesJson))
                return $"Было: {log.OldValuesJson}, Стало: {log.NewValuesJson}";

            if (!string.IsNullOrEmpty(log.NewValuesJson))
                return log.NewValuesJson;

            return log.ActionType;
        }
    }
}