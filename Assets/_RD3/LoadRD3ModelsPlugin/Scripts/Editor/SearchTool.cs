using System;
using System.Collections.Generic;
using System.Linq;

namespace _RD3.LoadRD3ModelsPlugin.Scripts.Editor
{
    public class SearchTool
    {
        public static List<FileEntry> SearchBy(List<FileEntry> allEntries, string searchText = "")
        {
            var currenEntries = allEntries;
            
            var filteredEntries = new List<FileEntry>();
            foreach (var entry in currenEntries.Where(entry => entry.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !filteredEntries.Contains(entry)))
                filteredEntries.Add(entry);
            
            foreach (var entry in currenEntries.Where(entry => entry.Tag.Contains(searchText.ToLower(),StringComparison.OrdinalIgnoreCase) && !filteredEntries.Contains(entry)))
                filteredEntries.Add(entry);

            foreach (var entry in currenEntries.Where(entry => entry.Project.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !filteredEntries.Contains(entry)))
                filteredEntries.Add(entry);
            
            
            return filteredEntries;
        }
        
        public static List<FileEntry> SearchByCategory(List<FileEntry> allEntries, Category category)
        {
            if (category == Category.Todos)
            {
                return new List<FileEntry>(allEntries);
            }

            string categoryText = category.ToString().Replace(" ", "").ToLowerInvariant();
            
            return allEntries
                .Where(entry =>
                    entry.Category.ToString().Replace(" ", "").ToLowerInvariant().Contains(categoryText))
                .ToList();
        }

    }
}