// Reporting component for Digital Stokvel Web Dashboard
// Task 3.4.6: Reporting and Export Functionality

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { ReportType, ExportFormat, ReportMetadata } from '../types/report';
import {
  generateReport,
  getGroupReports,
  downloadReport,
  getReportTypeDisplay,
  getReportTypeIcon,
  getFormatIcon,
  formatFileSize,
  formatDateTime,
  getDateRangePresets,
} from '../services/reportService';
import './Reporting.css';

const Reporting: React.FC = () => {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const { token } = useAuth();

  const [groupName, setGroupName] = useState<string>('');
  const [selectedReportType, setSelectedReportType] = useState<ReportType>('contribution_history');
  const [selectedFormat, setSelectedFormat] = useState<ExportFormat>('pdf');
  const [fromDate, setFromDate] = useState<string>('');
  const [toDate, setToDate] = useState<string>('');
  const [generatedReports, setGeneratedReports] = useState<ReportMetadata[]>([]);
  
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Initialize dates to last month
  useEffect(() => {
    const presets = getDateRangePresets();
    const lastMonth = presets.find(p => p.label === 'Last Month');
    if (lastMonth) {
      setFromDate(lastMonth.from);
      setToDate(lastMonth.to);
    }
  }, []);

  // Fetch group data and generated reports
  useEffect(() => {
    if (!groupId || !token) return;

    const fetchData = async () => {
      setLoading(true);
      setError(null);

      try {
        // Fetch group name (mock for now)
        setGroupName('Ntombizodwa Savings');

        // Fetch generated reports
        const reportsResponse = await getGroupReports(groupId, token);
        setGeneratedReports(reportsResponse.reports);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load reports');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [groupId, token]);

  const handleGenerateReport = async () => {
    if (!groupId || !token) return;

    setGenerating(true);
    setError(null);
    setSuccess(null);

    try {
      const reportMetadata = await generateReport(
        {
          group_id: groupId,
          report_type: selectedReportType,
          format: selectedFormat,
          filters: {
            from_date: fromDate,
            to_date: toDate,
          },
        },
        token
      );

      setSuccess(`Report generated successfully! Downloading...`);
      
      // Add to list of generated reports
      setGeneratedReports(prev => [reportMetadata, ...prev]);

      // Trigger download
      await downloadReport(reportMetadata.file_url);

      // Clear success message after 5 seconds
      setTimeout(() => setSuccess(null), 5000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate report');
    } finally {
      setGenerating(false);
    }
  };

  const handleDownload = async (report: ReportMetadata) => {
    try {
      await downloadReport(report.file_url);
    } catch (err) {
      setError(`Failed to download report: ${err instanceof Error ? err.message : 'Unknown error'}`);
    }
  };

  const handleDatePreset = (preset: { from: string; to: string }) => {
    setFromDate(preset.from);
    setToDate(preset.to);
  };

  const reportTypes: Array<{ value: ReportType; label: string; icon: string }> = [
    { value: 'contribution_history', label: 'Contribution History', icon: '💰' },
    { value: 'payout_history', label: 'Payout History', icon: '📤' },
    { value: 'member_activity', label: 'Member Activity', icon: '👥' },
    { value: 'group_ledger', label: 'Group Ledger', icon: '📊' },
    { value: 'annual_summary', label: 'Annual Summary', icon: '📅' },
  ];

  const formats: Array<{ value: ExportFormat; label: string; icon: string }> = [
    { value: 'pdf', label: 'PDF', icon: '📄' },
    { value: 'csv', label: 'CSV', icon: '📋' },
    { value: 'excel', label: 'Excel', icon: '📗' },
  ];

  if (loading) {
    return (
      <div className="reporting">
        <div className="loading">Loading reports...</div>
      </div>
    );
  }

  return (
    <div className="reporting">
      <div className="reporting-header">
        <button className="back-button" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </button>
        <h1>Reports & Export</h1>
        <p className="group-name">{groupName}</p>
      </div>

      {error && (
        <div className="error-message">
          <span className="error-icon">⚠️</span>
          {error}
        </div>
      )}

      {success && (
        <div className="success-message">
          <span className="success-icon">✓</span>
          {success}
        </div>
      )}

      <div className="reporting-content">
        {/* Report Generator Section */}
        <div className="report-generator">
          <h2>Generate Report</h2>
          
          <div className="form-group">
            <label htmlFor="report-type">Report Type</label>
            <select
              id="report-type"
              value={selectedReportType}
              onChange={(e) => setSelectedReportType(e.target.value as ReportType)}
              className="report-type-select"
            >
              {reportTypes.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.icon} {type.label}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Date Range</label>
            <div className="date-range-presets">
              {getDateRangePresets().map((preset) => (
                <button
                  key={preset.label}
                  className="preset-button"
                  onClick={() => handleDatePreset(preset)}
                >
                  {preset.label}
                </button>
              ))}
            </div>
            <div className="date-inputs">
              <div className="date-input-group">
                <label htmlFor="from-date">From</label>
                <input
                  id="from-date"
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                />
              </div>
              <div className="date-input-group">
                <label htmlFor="to-date">To</label>
                <input
                  id="to-date"
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                />
              </div>
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="format">Export Format</label>
            <div className="format-options">
              {formats.map((format) => (
                <label key={format.value} className="format-option">
                  <input
                    type="radio"
                    name="format"
                    value={format.value}
                    checked={selectedFormat === format.value}
                    onChange={(e) => setSelectedFormat(e.target.value as ExportFormat)}
                  />
                  <span className="format-label">
                    <span className="format-icon">{format.icon}</span>
                    {format.label}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <button
            className="generate-button"
            onClick={handleGenerateReport}
            disabled={generating || !fromDate || !toDate}
          >
            {generating ? (
              <>
                <span className="spinner"></span>
                Generating...
              </>
            ) : (
              <>
                <span className="button-icon">📊</span>
                Generate & Download Report
              </>
            )}
          </button>
        </div>

        {/* Generated Reports List */}
        <div className="generated-reports">
          <h2>Previously Generated Reports</h2>
          
          {generatedReports.length === 0 ? (
            <div className="empty-state">
              <span className="empty-icon">📁</span>
              <p>No reports generated yet</p>
              <p className="empty-hint">Generate your first report using the form above</p>
            </div>
          ) : (
            <div className="reports-list">
              {generatedReports.map((report) => (
                <div key={report.id} className="report-item">
                  <div className="report-icon">
                    {getReportTypeIcon(report.report_type)}
                  </div>
                  <div className="report-details">
                    <div className="report-title">
                      {getReportTypeDisplay(report.report_type)}
                    </div>
                    <div className="report-meta">
                      <span className="report-format">
                        {getFormatIcon(report.format)} {report.format.toUpperCase()}
                      </span>
                      <span className="report-size">
                        {formatFileSize(report.file_size_bytes)}
                      </span>
                      <span className="report-date">
                        {formatDateTime(report.generated_at)}
                      </span>
                    </div>
                    {report.filters.from_date && report.filters.to_date && (
                      <div className="report-period">
                        Period: {new Date(report.filters.from_date).toLocaleDateString()} - {new Date(report.filters.to_date).toLocaleDateString()}
                      </div>
                    )}
                  </div>
                  <button
                    className="download-button"
                    onClick={() => handleDownload(report)}
                  >
                    <span className="download-icon">⬇️</span>
                    Download
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Reporting;
