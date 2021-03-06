﻿// Decompiled with JetBrains decompiler
// Type: CanalCocina3.CanalCocina
// Assembly: CanalCocina3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9D302122-99E7-4337-B8A8-DD06FFD49964
// Assembly location: C:\Users\xavi\Downloads\CanalCocina3.dll

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CanalCocina3
{
  public class CanalCocina
  {
    private string urlcanalbase = "http://canalcocina.es/television/guia-tv/?date=";

    public bool Init()
    {
      return true;
    }

		/// <summary>
		/// Instancia un nou HttpDownloader
		/// </summary>
		/// <param name="day">Un dia</param>
		/// <returns>Retorna la string de la url de la pàgina web</returns>

    public string GetRawHtmlPageProgramation(DateTime day)
    {
      return new HttpDownloader(this.urlcanalbase + day.Day.ToString("D2") + "-" + day.Month.ToString("D2") + "-" + day.Year.ToString("D4"), (string) null, (string) null)
      {
        Encoding = Encoding.UTF8
      }.GetPage();
    }

		/// <summary>
		/// Crida al metode que li passarà la programació de del dia
		/// </summary>
		/// <param name="day">Data del dia elegit</param>
		/// <returns>Torna una llista de la classe Programa</returns>
    public List<Program> Get(DateTime day)
    {
      return this.GetProgramsFromHtml(this.GetRawHtmlPageProgramation(day), day);
    }

		/// <summary>
		/// Carrega codi html amb la string que li has passat, i per cada programa que va trobant posa la informació dins de la classe program
		/// </summary>
		/// <param name="htmlContent">url de la pàgina que vols agafar l'html</param>
		/// <param name="baseday">Data del dia</param>
		/// <returns>retorna una llista de la classe Program</returns>
    public List<Program> GetProgramsFromHtml(string htmlContent, DateTime baseday)
    {
      List<Program> programList = new List<Program>();
      HtmlDocument htmlDocument = new HtmlDocument();
      htmlDocument.LoadHtml(htmlContent);
      HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//section[@class='recipe-content']/a");
      if (htmlNodeCollection == null || htmlNodeCollection.Count == 0)
        return (List<Program>) null;
      DateTime minValue = DateTime.MinValue;
      HtmlNodeCollection.HtmlNodeEnumerator enumerator = htmlNodeCollection.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Program programFromHtmlNode = this.CreateProgramFromHtmlNode(enumerator.Current, baseday);
          if (programFromHtmlNode.Start.HasValue)
          {
            DateTime? start = programFromHtmlNode.Start;
            DateTime dateTime = minValue;
            if ((start.HasValue ? (start.GetValueOrDefault() < dateTime ? 1 : 0) : 0) != 0)
              baseday = baseday.AddDays(1.0);
            minValue = programFromHtmlNode.Start.Value;
            programList.Add(programFromHtmlNode);
          }
        }
      }
      finally
      {
        (enumerator as IDisposable)?.Dispose();
      }
      return programList;
    }
		/// <summary>
		/// Va agafant els nodes del codi html que conté informació per crear la classe Program
		/// </summary>
		/// <param name="htmlnode">Tots els nodes que han concideixen amb el node html demanat</param>
		/// <param name="basedate">Data del dia</param>
		/// <returns>Retorna una classe Program</returns>
		public Program CreateProgramFromHtmlNode(HtmlNode htmlnode, DateTime basedate)
    {
      Program program = new Program();
      HtmlNode htmlNode1 = htmlnode.SelectSingleNode(".//p[@class='hour']");
      HtmlNode htmlNode2 = htmlnode.SelectSingleNode(".//span");
      HtmlNode htmlNode3 = htmlnode.SelectSingleNode(".//p[@class='subtitle']");
      HtmlNode htmlNode4 = htmlnode.SelectSingleNode("(.//p)[last()]");
      if (htmlNode1 == null || htmlNode2 == null)
        return (Program) null;
      string[] strArray = Regex.Replace(htmlNode1.InnerText, "[^0-9:]", "").Split(':');
      DateTime dateTime = new DateTime(basedate.Year, basedate.Month, basedate.Day, int.Parse(strArray[0]), int.Parse(strArray[1]), 0);
      program.Start = new DateTime?(dateTime);
      string input = htmlNode2.InnerText;
      string str = htmlNode3.InnerText;
      Match match = Regex.Match(input, " T(?<temporada>[0-9]+)\\s*$");
      if (match.Success)
      {
        program.Session = int.Parse(match.Groups["temporada"].Value);
        input = Regex.Replace(input, " T(?<temporada>[0-9]+)\\s*$", "");
      }
      int result = -1;
      if (!string.IsNullOrEmpty(str) && str.Length > 9 && str.Contains("Episodio "))
      {
        if (int.TryParse(str.Substring(9), out result))
          str = "";
        else
          result = -1;
      }
      if (result >= 0)
      {
        program.Chapter = result;
        program.Title = input;
      }
      else
        program.Title = input + str;
      program.Desc = htmlNode4 == null ? string.Empty : Regex.Replace(htmlNode4.InnerHtml, "\\s+", " ", RegexOptions.Multiline).Trim();
      return program;
    }
  }
}
