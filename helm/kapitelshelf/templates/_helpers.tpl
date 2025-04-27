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
app.kubernetes.io/name: {{ include "kapitelshelf.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{- define "postgresql.host" -}}
{{- if .Values.global.deployPostgres -}}
{{ include "kapitelshelf.name" . }}-postgresql.{{ .Values.global.namespace }}.svc.cluster.local:{{ .Values.postgresql.primary.service.port }}
{{- else -}}
{{ .Values.postgres.host }}
{{- end -}}
{{- end -}}