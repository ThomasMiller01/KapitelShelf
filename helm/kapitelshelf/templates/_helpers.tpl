{{- define "kapitelshelf.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "kapitelshelf.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name (include "kapitelshelf.name" .) | trunc 63 | trimSuffix "-" }}
{{- end -}}
{{- end -}}

{{- define "kapitelshelf.labels" -}}
labels:
  app.kubernetes.io/name: {{ include "kapitelshelf.name" . }}
  app.kubernetes.io/instance: {{ .Release.Name }}
  app.kubernetes.io/version: {{ .Chart.AppVersion }}
  app.kubernetes.io/managed-by: Helm
{{- end -}}

{{- define "postgresql.host" -}}
{{- if .Values.global.deploypostgres -}}
{{ include "kapitelshelf.fullname" . }}-postgres:{{ .Values.postgres.service.port }}
{{- else -}}
{{ .Values.postgres.host }}
{{- end -}}
{{- end -}}