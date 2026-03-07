{{/* --- k8s Helpers --- */}}
{{- define "kapitelshelf.name" -}}
{{- default .Chart.Name .Values.nameOverride | lower | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "kapitelshelf.labels" -}}
app.kubernetes.io/name: {{ include "kapitelshelf.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{- define "kapitelshelf.annotations" -}}
{{- if . -}}
{{- toYaml . -}}
{{- end -}}
{{- end -}}

{{/* --- API Helpers --- */}}
{{- define "kapitelshelf.api.host" -}}
{{- if .Values.api.ingress.enabled -}}
    {{- if .Values.api.ingress.tls -}}
https://{{ .Values.api.ingress.host }}{{ .Values.api.ingress.path }}
    {{- else -}}
http://{{ .Values.api.ingress.host }}{{ .Values.api.ingress.path }}
    {{- end -}}
{{- else -}}
http://localhost:{{ .Values.api.service.port }}
{{- end -}}
{{- end -}}


{{/* --- Database Helpers --- */}}
{{- define "kapitelshelf.database.host" -}}
{{- if .Values.global.deployDatabase -}}
{{ include "kapitelshelf.name" . }}-database
{{- else if .Values.global.deployPostgresql -}} {{/* [DEPRECATED] */}}
{{ .Release.Name }}-postgresql.{{ .Values.global.namespace }}.svc.cluster.local
{{- else -}}
{{ .Values.postgresql.primary.service.host }}{{/* In a future release, fallback to {{ .Values.database.service.host }} */}}
{{- end -}}
{{- end -}}

{{/* [DEPRECATED] because of deployPostgresql, when removed just use `.Values.database.service.port` */}}
{{- define "kapitelshelf.database.port" -}}
{{- if .Values.global.deployDatabase -}}
{{ .Values.database.service.port }}
{{- else if .Values.global.deployPostgresql -}}
{{ .Values.postgresql.primary.service.port }}
{{- else -}}
{{ .Values.postgresql.primary.service.port }}
{{- end -}}
{{- end -}}

{{/* [DEPRECATED] because of deployPostgresql, when removed just use `.Values.database.auth.username` */}}
{{- define "kapitelshelf.database.username" -}}
{{- if .Values.global.deployDatabase -}}
{{ .Values.database.auth.username }}
{{- else if .Values.global.deployPostgresql -}}
{{ .Values.postgresql.auth.username }}
{{- else -}}
{{ .Values.postgresql.auth.username }}
{{- end -}}
{{- end -}}

{{/* [DEPRECATED] because of deployPostgresql, when removed just use `.Values.database.auth.password` */}}
{{- define "kapitelshelf.database.password" -}}
{{- if .Values.global.deployDatabase -}}
{{ .Values.database.auth.password }}
{{- else if .Values.global.deployPostgresql -}}
{{ .Values.postgresql.auth.password }}
{{- else -}}
{{ .Values.postgresql.auth.password }}
{{- end -}}
{{- end -}}