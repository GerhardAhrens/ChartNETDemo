# Chart Controls in WPF für NET

[![NET Framework](https://img.shields.io/badge/NET%20Core-%2010-red.svg)](#)
[![Version](https://img.shields.io/badge/Version-%201.0.2026.1-blue.svg)](#)
![License](https://img.shields.io/badge/License-MIT-yellow.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2026-white.svg)

Das Projekt soll Beispielcode und Anleitungen für die Verwendung von Chart Controls in WPF-Anwendungen bereitstellen, die auf dem NET 10 basieren. Die Chart Controls sind ohne zusätzliche Bibliotheken in C# erstellt.

Die Chart Controls sind mit absicht einfach gehalten, um die Grundfunktionalitäten zu demonstrieren und können als Ausgangspunkt für komplexere Implementierungen dienen. So können die Chart Controls ohne großen Aufwand komplett als UnserControl in eigene WPF Projekt hinzugefügt werden. Daher sind nur einfache Grundfunktionen vorhanden, plus die Möglichkeit die Charts als PNG Datei zu exportieren.

In einem Demo-Projekt sind die verschiedenen Chart Controls implementiert und können direkt in einer WPF-Anwendung verwendet werden.

Implementierte Diagrammtypen:\
| Diagrammtyp        | Beschreibung                              |
|:--------------------|:-------------------------------------------|
| Line Chart     | Darstellung von Datenpunkten als Linie    |
| Bar Chart     | Darstellung von Daten als Balken          |
| Bar Chart Horizontal | Darstellung von Daten als Balken, horizontal |
| Column Chart  | Darstellung von Daten als Balken, in Kategorien gruppiert |
| Pie Chart      | Darstellung von Daten als Segmente eines Kreises |
| Gantt Chart    | Darstellung von Daten als gefüllte Fläche |
| Treemap Chart    | Darstellung von Daten im Größenverhältnis |
| Headmap Chart    | Darstellung von Daten im Verhältnis der Farbinensität |
| Scatter Chart    | Darstellung von Daten in einem Streuungsdiagramm |

Verschiedene Beispiele der Charts
- Line Chart
<img src="LineChartDemo.png" style="width:650px;"/>

- Gantt Chart
<img src="GanttChartDemo.png" style="width:650px;"/>

- Treemap Chart
<img src="TreeMapChartDemo.png" style="width:650px;"/>

- Headmap Chart
<img src="HeadmapChartDemo.png" style="width:650px;"/>

- Scatter Chart
<img src="ScatterChartDemo.png" style="width:650px;"/>

## Versionshistorie
[![Versionshistorie](https://img.shields.io/badge/Version-%201.0.2026.3-blue.svg)](#)
- Scatter Chart Control (Streuungsdiagramm) hinzugefügt

[![Versionshistorie](https://img.shields.io/badge/Version-%201.0.2026.2-blue.svg)](#)
- Zu allen Charts wurde eine Tooltip Funktion hinzugefügt.